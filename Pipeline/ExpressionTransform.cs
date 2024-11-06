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
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class ExpressionTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource
    {
        [Import]
        public IConditionBuilder ConditionBuilder { get; set; }
        public static ISettingDefinition ProjectionExpression = new SimpleSettingDefinition("ProjectionExpression", "Pipeline.Transform", "Expression", "Expression to turn row into in condition expression language", null, null, false, 20);
        public ExpressionTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.Expression", "Projection Expression", typeof(ExpressionTransform).FullName, "DevTools.Pipeline.Transform"), new String[] {ProjectionExpression.FullKey()})
        { }

        class Transform : HostedObjectBase, IHtValueTransform
        {
            IHtExpression _expression;
            internal Transform(IHtExpression expression) {
                _expression = expression;
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                return input.Select(row => _expression.Evaluate(row)??new JsonHtValue());
            }
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            string expr = state.SettingValues[ProjectionExpression.FullKey()];
            return new Transform(ConditionBuilder.ParseCondition(expr));
        }

        public override string NodeDescription
        {
            get { return ProjectionExpression.Value<string>(SettingManager); }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {ProjectionExpression}; }
        }
    }
}
