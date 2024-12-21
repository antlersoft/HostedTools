using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.Framework.Model;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Model.Plugin.Internal;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IStemNode))]
    public class OrderByTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource
    {
        [Import] public IConditionBuilder ConditionBuilder;

        public static ISettingDefinition OrderByList = new SimpleSettingDefinition("OrderByList", "Pipeline.Transform", "Order by", "Comma-separated list of expressions to order by");
        public static ISettingDefinition Descending = new SimpleSettingDefinition("Descending", "Pipeline.Transform", "Descending", null, typeof(bool), "false", false, 0);
        public static ISettingDefinition NullsLast = new SimpleSettingDefinition("NullsLast", "Pipeline.Transform", "Nulls last", null, typeof(bool), "false", false, 0);
        public OrderByTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.OrderBy", "Order by", typeof(OrderByTransform).FullName, "DevTools.Pipeline.Transform"),
            new[] { OrderByList.FullKey(), Descending.FullKey(), NullsLast.FullKey()})
        {
            
        }

        internal class Transform : HostedObjectBase, IHtValueTransform, IDisposable {
            private readonly string _orderByList;
            private readonly bool _descending;
            private readonly bool _nullsLast;
            static readonly int MaxOrderSize = 200000;
            private readonly IConditionBuilder _conditionBuilder;
            private readonly IPluginManager _pluginManager;
            private readonly List<IDisposable> _subTransforms = new List<IDisposable>();

            internal Transform(IPluginManager p, string orderByList, bool descending, bool nullsLast, IConditionBuilder conditionBuilder) {
                _orderByList = orderByList;
                _descending = descending;
                _nullsLast = nullsLast;
                _conditionBuilder = conditionBuilder;
                _pluginManager = p;
            }

            struct MergeDictKey {
                internal IHtValue val;
                internal int index;

                internal MergeDictKey(IHtValue v, int i) {
                    val = v;
                    index = i;
                }

                internal class MergeDictComparer : IComparer<MergeDictKey>
                {
                    IComparer<IHtValue> _underlying;
                    internal MergeDictComparer(IComparer<IHtValue> underlying) {
                        _underlying = underlying;
                    }
                    int IComparer<MergeDictKey>.Compare(MergeDictKey x, MergeDictKey y)
                    {
                        int result = _underlying.Compare(x.val, y.val);
                        if (result == 0) {
                            result = x.index - y.index;
                        }
                        return result;
                    }
                }
            }

            internal IEnumerable<IHtValue> MergeSort(List<IEnumerable<IHtValue>> subTasks, IComparer<IHtValue> comparer, ICancelableMonitor monitor) {
                int index = 0;
                var enumerators = subTasks.Select(t => t.GetEnumerator()).ToArray();
                var queue = new SortedDictionary<MergeDictKey,int>(new MergeDictKey.MergeDictComparer(comparer));
                foreach (var x in enumerators) {
                    if (x.MoveNext()) {
                        queue.Add(new MergeDictKey(x.Current,index),index);
                    }
                    index++;
                }
                while (queue.Count > 0 && ! (monitor?.IsCanceled ?? false)) {
                    var key = queue.Keys.First();
                    queue.Remove(key);
                    if (enumerators[key.index].MoveNext()) {
                        queue.Add(new MergeDictKey(enumerators[key.index].Current, key.index), key.index);
                    }
                    yield return key.val;
                }
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                if (string.IsNullOrWhiteSpace(_orderByList))
                {
                    return input;
                }
                ICancelableMonitor cancelable = monitor.Cast<ICancelableMonitor>();
                List<IHtExpression> sortExpressions = new List<IHtExpression>();
                foreach (string exprStr in _orderByList.Split(','))
                {
                    sortExpressions.Add(_conditionBuilder.ParseCondition(exprStr));
                }
                var comparer = new OrderByComparer(
                        new ValueComparer(_descending, _nullsLast),
                        sortExpressions);

                var subTasks = new List<IEnumerable<IHtValue>>();
                var current = new IHtValue[MaxOrderSize];
                TempFileTransform tft = null;
                int count = 0;
                using (var e = input.GetEnumerator()) {
                    for (; e.MoveNext() && ! (cancelable?.IsCanceled ?? false); ) {
                        current[count++] = e.Current;
                        if (count == MaxOrderSize) {
                            Array.Sort(current, comparer);
                            if (tft == null) {
                                tft = _pluginManager[typeof(TempFileTransform).FullName] as TempFileTransform;
                            }
                            var transform = tft.GetTempFileTransform();
                            if (transform.Cast<IDisposable>() is IDisposable sub) {
                                _subTransforms.Add(sub);
                            }
                            subTasks.Add(transform.GetTransformed(current, monitor));
                            count = 0;
                        }
                    }
                }
                Array.Resize(ref current, count);
                Array.Sort(current, comparer);
                if (subTasks.Count == 0) {
                    return current;
                }
                subTasks.Add(current);
                return MergeSort(subTasks, comparer, cancelable);
            }

            public void Dispose()
            {
                foreach (var t in _subTransforms) {
                    t.Dispose();
                }
            }
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(
                PluginManager,
                state.SettingValues[OrderByList.FullKey()],
                (bool)Convert.ChangeType(state.SettingValues[Descending.FullKey()], typeof(bool)),
                (bool)Convert.ChangeType(state.SettingValues[NullsLast.FullKey()], typeof(bool)),
                ConditionBuilder
            );
        }

        public override string NodeDescription
        {
            get
            {
                return "Order by " + OrderByList.Value<string>(SettingManager) +
                       (Descending.Value<bool>(SettingManager) ? " descending" : string.Empty) +
                       (NullsLast.Value<bool>(SettingManager) ? " nulls last" : string.Empty);
            }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new [] {OrderByList, Descending, NullsLast}; }
        }
    }
}
