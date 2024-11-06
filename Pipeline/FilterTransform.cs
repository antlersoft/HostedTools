using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class FilterTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource
    {
        [Import]
        public IConditionBuilder ConditionBuilder { get; set; }
        public static ISettingDefinition FilterExpression = new SimpleSettingDefinition("FilterExpression", "Pipeline.Transform", "Filter", "Boolean filter on row in condition expression language", null, null, false, 20);
        public FilterTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.Filter", "Filter Expression", typeof(FilterTransform).FullName, "DevTools.Pipeline.Transform"), new String[] {FilterExpression.FullKey() })
        { }

        class Transform : HostedObjectBase, IHtValueTransform {
            IHtExpression _expression;
            internal Transform(IHtExpression expression) {
                _expression = expression;
            }
            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                return input.Where(row => _expression.Evaluate(row).AsBool);
            }
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            string expr = FilterExpression.Value<string>(SettingManager);
            IHtExpression expression = ConditionBuilder.ParseCondition(expr);
            return new Transform(expression);
        }

        public override string NodeDescription
        {
            get { return FilterExpression.Value<string>(SettingManager); }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] { FilterExpression }; }
        }
    }
}
