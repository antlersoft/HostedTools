﻿using System;
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
    public class GroupByTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource
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

        class Transform : HostedObjectBase, IHtValueTransform, IDisposable {
            private readonly IPluginManager _pluginManager;
            private readonly string _sortString;
            private readonly string _projectionExpression;
            private readonly bool _needSort;
            private readonly IConditionBuilder _conditionBuilder;
            private IDisposable _disposable;

            internal Transform(IPluginManager p, string sortString, string projectionExpression, bool needSort, IConditionBuilder conditionBuilder) {
                _pluginManager = p;
                _sortString = sortString;
                _projectionExpression = projectionExpression;
                _needSort = needSort;
                _conditionBuilder = conditionBuilder;
            }

            public void Dispose()
            {
                _disposable?.Dispose();
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                List<IHtExpression> sortExpressions = new List<IHtExpression>();
                foreach (string exprStr in _sortString.Split(','))
                {
                    sortExpressions.Add(_conditionBuilder.ParseCondition(exprStr));
                }

                IHtValue currentRow=null;
                IHtExpression pe=String.IsNullOrWhiteSpace(_projectionExpression) ? null : _conditionBuilder.ParseCondition(_projectionExpression);
                IGroupExpression groupExpression = pe as IGroupExpression;
                IComparer<IHtValue> groupComparer = new OrderByComparer(
                        new ValueComparer(),
                        sortExpressions);
                IEnumerable<IHtValue> sortedInput = input;
                if (_needSort)
                {
                    var sortedTransform = new OrderByTransform.Transform(_pluginManager, _sortString, false, false, _conditionBuilder);
                    _disposable = sortedTransform;
                    sortedInput = sortedTransform.GetTransformed(sortedInput, monitor);
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
        }

        public override string NodeDescription
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

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(PluginManager, state.SettingValues[GroupByList.FullKey()],
                state.SettingValues[GroupProjectionExpression.FullKey()],
                (bool)Convert.ChangeType(state.SettingValues[NeedSort.FullKey()], typeof(bool)),
                ConditionBuilder);
        }
    }
}
