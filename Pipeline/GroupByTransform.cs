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
    public class GroupByTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
    {
        [Import] public IConditionBuilder ConditionBuilder;

        public static ISettingDefinition GroupByList = new SimpleSettingDefinition("GroupByList", "Pipeline.Transform", "Group by", "Comma-separated list of expressions to group by");
        public static ISettingDefinition GroupProjectionExpression = new SimpleSettingDefinition("GroupProjectionExpression", "Pipeline.Transform", "Projection expression with output of grouping");
        public static ISettingDefinition NeedSort = new SimpleSettingDefinition("NeedSort", "Pipeline.Transform", "Need sort", "If checked, sort before grouping", typeof(bool), "true", false, 0);
        public GroupByTransform()
            : base(new MenuItem("DevTools.Pipeline.Transform.OrderBy", "Order by", typeof(OrderByTransform).FullName, "DevTools.Pipeline.Transform"),
            new[] { GroupByList.FullKey(), GroupProjectionExpression.FullKey(), NeedSort.FullKey()})
        {
            
        }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            string s = GroupByList.Value<string>(SettingManager);
            List<IHtExpression> sortExpressions = new List<IHtExpression>();
            foreach (string exprStr in s.Split(','))
            {
                sortExpressions.Add(ConditionBuilder.ParseCondition(exprStr));
            }

            IHtValue currentRow=null;
            IHtExpression pe=ConditionBuilder.ParseCondition(GroupProjectionExpression.Value<string>(SettingManager));
            IGroupExpression groupExpression = pe as IGroupExpression;
            IComparer<IHtValue> groupComparer = new OrderByComparer(
                    new ValueComparer(),
                    sortExpressions);
            IEnumerable<IHtValue> sortedInput = input;
            if (NeedSort.Value<bool>(SettingManager))
            {
                sortedInput = input.OrderBy(row => row, groupComparer);
            }
            foreach (var r in sortedInput)
            {
                if (currentRow == null)
                {
                    currentRow = r;
                    if (groupExpression!= null) {
                        groupExpression.StartGroup();
                        groupExpression.AddRow(currentRow);
                    }
                } else if (sortExpressions.Count > 0 && groupComparer.Compare(currentRow, r) == 0)
                {
                    if (groupExpression != null)
                    {
                        groupExpression.AddRow(r);
                    }
                    currentRow = r;
                } else {
                    if (groupExpression != null)
                    {
                        groupExpression.EndGroup();
                    }
                    if (pe !=null) {
                        yield return pe.Evaluate(currentRow);
                    } else {
                        yield return currentRow;
                    }                   
                    currentRow = r;
                    if (groupExpression != null)
                    {
                        groupExpression.StartGroup();
                        groupExpression.AddRow(currentRow);
                    }   
                }
            }
            if (currentRow != null) {
                if (groupExpression != null)
                {
                    groupExpression.EndGroup();
                }
                if (pe !=null) {
                    yield return pe.Evaluate(currentRow);
                } else {
                    yield return currentRow;
                }                   
            }
        }

        public string TransformDescription
        {
            get
            {
                return $"Group by {GroupByList.Value<string>(SettingManager)} returning {GroupProjectionExpression.Value<string>(SettingManager)}";
            }
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new [] {GroupByList, GroupProjectionExpression, NeedSort}; }
        }
    }
}
