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

namespace com.antlersoft.HostedTools.Pipeline
{
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
