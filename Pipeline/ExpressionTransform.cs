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
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueTransform))]
    [Export(typeof(ISettingDefinitionSource))]
    public class ExpressionTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
    {
        [Import]
        public IConditionBuilder ConditionBuilder { get; set; }
        public static ISettingDefinition ProjectionExpression = new SimpleSettingDefinition("ProjectionExpression", "Pipeline.Transform", "Expression", "Expression to turn row into in condition expression language", null, null, false, 20);
        public ExpressionTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.Expression", "Projection Expression", typeof(ExpressionTransform).FullName, "DevTools.Pipeline.Transform"), new String[] {ProjectionExpression.FullKey()})
        { }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            string expr = ProjectionExpression.Value<string>(SettingManager);
            IHtExpression expression = ConditionBuilder.ParseCondition(expr);
            return input.Select(row => expression.Evaluate(row)??new JsonHtValue());
        }

        public string TransformDescription
        {
            get { return ProjectionExpression.Value<string>(SettingManager); }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {ProjectionExpression}; }
        }
    }
}
