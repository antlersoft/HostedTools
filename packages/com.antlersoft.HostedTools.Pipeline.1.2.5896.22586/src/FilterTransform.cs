using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueTransform))]
    [Export(typeof(ISettingDefinitionSource))]
    public class FilterTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
    {
        [Import]
        public IConditionBuilder ConditionBuilder { get; set; }
        public static ISettingDefinition FilterExpression = new SimpleSettingDefinition("FilterExpression", "Pipeline.Transform", "Filter", "Boolean filter on row in condition expression language", null, null, false, 20);
        public FilterTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.Filter", "Filter Expression", typeof(FilterTransform).FullName, "DevTools.Pipeline.Transform"), new String[] {FilterExpression.FullKey() })
        { }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            string expr = FilterExpression.Value<string>(SettingManager);
            IHtExpression expression = ConditionBuilder.ParseCondition(expr);
            return input.Where(row => expression.Evaluate(row).AsBool);
        }

        public string TransformDescription
        {
            get { return FilterExpression.Value<string>(SettingManager); }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] { FilterExpression }; }
        }
    }
}
