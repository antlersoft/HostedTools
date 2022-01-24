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

namespace com.antlersoft.HostedTools.Pipeline
{
    class ValueComparer : IComparer<IHtValue>
    {
        private bool _nullsLast;
        private bool _descending;

        internal ValueComparer(bool descending = false, bool nullsLast = false)
        {
            _descending = descending;
            _nullsLast = nullsLast;
        }

        public int Compare(IHtValue x, IHtValue y)
        {
            if (x.IsEmpty)
            {
                if (y.IsEmpty)
                {
                    return 0;
                }
                return _nullsLast ? 1 : -1;
            }
            if (y.IsEmpty)
            {
                return _nullsLast ? -1 : 1;
            }
            int diff;
            if (x.IsBool && y.IsBool)
            {
                diff = (x.AsBool ? 1 : 0) - (y.AsBool ? 1 : 0);
            }
            else if (x.IsDouble && y.IsDouble)
            {
                diff = x.AsDouble > y.AsDouble ? 1 : (y.AsDouble > x.AsDouble ? -1 : 0);
            }
            else if (x.IsLong && y.IsLong)
            {
                diff = x.AsLong > y.AsLong ? 1 : (y.AsLong > x.AsLong ? -1 : 0);
            }
            else
            {
                diff = String.CompareOrdinal(x.AsString, y.AsString);
            }
            return _descending ? -diff : diff;
        }
    }

    class OrderByComparer : IComparer<IHtValue>
    {
        private ValueComparer _values;
        private List<IHtExpression> _expressions;
        internal OrderByComparer(ValueComparer values, List<IHtExpression> expressions)
        {
            _values = values;
            _expressions = expressions;
        }

        public int Compare(IHtValue x, IHtValue y)
        {
            foreach (var expr in _expressions)
            {
                int diff = _values.Compare(expr.Evaluate(x), expr.Evaluate(y));
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }
    }

    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IHtValueTransform))]
    public class OrderByTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
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

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            string s = OrderByList.Value<string>(SettingManager);
            if (String.IsNullOrWhiteSpace(s))
            {
                return input;
            }
            List<IHtExpression> sortExpressions = new List<IHtExpression>();
            foreach (string exprStr in s.Split(','))
            {
                sortExpressions.Add(ConditionBuilder.ParseCondition(exprStr));
            }

            return input.OrderBy(row => row,
                new OrderByComparer(
                    new ValueComparer(Descending.Value<bool>(SettingManager), NullsLast.Value<bool>(SettingManager)),
                    sortExpressions));
        }

        public string TransformDescription
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
