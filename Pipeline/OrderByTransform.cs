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

        class Transform : HostedObjectBase, IHtValueTransform {
            private readonly string _orderByList;
            private readonly bool _descending;
            private readonly bool _nullsLast;
            private readonly IConditionBuilder _conditionBuilder;

            internal Transform(string orderByList, bool descending, bool nullsLast, IConditionBuilder conditionBuilder) {
                _orderByList = orderByList;
                _descending = descending;
                _nullsLast = nullsLast;
                _conditionBuilder = conditionBuilder;
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                if (String.IsNullOrWhiteSpace(_orderByList))
                {
                    return input;
                }
                List<IHtExpression> sortExpressions = new List<IHtExpression>();
                foreach (string exprStr in _orderByList.Split(','))
                {
                    sortExpressions.Add(_conditionBuilder.ParseCondition(exprStr));
                }

                return input.OrderBy(row => row,
                    new OrderByComparer(
                        new ValueComparer(_descending, _nullsLast),
                        sortExpressions));
            }
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(
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
