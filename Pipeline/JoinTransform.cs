using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueTransform))]
    [Export(typeof(ISettingDefinitionSource))]
    public class JoinTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
    {
        public enum JoinTypes {
            FilterOnly,
            AllColumns,
            ProjectionExpression
        };

        static ISettingDefinition ToFilterJoinKey = new SimpleSettingDefinition("InputJoinKey", "Pipeline.JoinTransform", "Key in input to join on", "Input must be sorted on given key");
        static ISettingDefinition FileFilterJoinKey = new SimpleSettingDefinition("FileJoinKey", "Pipeline.JoinTransform", "Key in file data to join on", "Data file must be sorted on given key");
        static ISettingDefinition JoinType = new SimpleSettingDefinition("JoinType", "Pipeline.JoinTransform", "Join Type", null, typeof(JoinTypes), "FilterOnly", false, 0);
        static ISettingDefinition ProjectionExpression = new SimpleSettingDefinition("ProjectionExpression", "Pipeline.JoinTransform", "Projection Expression");

        [Import]
        public IConditionBuilder _expressionBuilder;
        [Import]
        public IJsonFactory _jsonFactory;

        private IComparer<IHtValue> _comparer = new ValueComparer();

        public JoinTransform()
        : base(new MenuItem("DevTools.Pipeline.Transform.JoinTransform", "Join Transform", typeof(JoinTransform).FullName, "DevTools.Pipeline.Transform"),
            new [] {Settings.LoadFile.FullKey(), Settings.GZipData.FullKey(), ToFilterJoinKey.FullKey(), FileFilterJoinKey.FullKey(), JoinType.FullKey(), ProjectionExpression.FullKey()})
        {

        }

        public IEnumerable<ISettingDefinition> Definitions => new [] { ToFilterJoinKey, FileFilterJoinKey, JoinType, ProjectionExpression};

        public string TransformDescription => $"join {Settings.LoadFile.Value<string>(SettingManager)} "+
            (JoinType.Value<JoinTypes>(SettingManager) == JoinTypes.ProjectionExpression ? "Projection " + ProjectionExpression.Value<string>(SettingManager)
            : JoinType.Value<string>(SettingManager));

        class Unchanged : IHtExpression
        {
            internal static Unchanged Instance = new Unchanged();
            public IHtValue Evaluate(IHtValue val)
            {
                return val;
            }
        }

        private IHtExpression GetKeyFromExpression(string expr)
        {
            if (String.IsNullOrWhiteSpace(expr))
            {
                return Unchanged.Instance;
            }
            return _expressionBuilder.ParseCondition(expr);
        }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
            IHtValue a, b;
            string path = Settings.LoadFile.Value<string>(SettingManager);
            bool isGzip = Settings.GZipData.Value<bool>(SettingManager);
            JoinTypes jt = JoinType.Value<JoinTypes>(SettingManager);
            IHtExpression projectionExpression = null;
            if (jt == JoinTypes.ProjectionExpression)
            {
                projectionExpression = _expressionBuilder.ParseCondition(ProjectionExpression.Value<string>(SettingManager));
            }

            IHtExpression filteredKey = GetKeyFromExpression(ToFilterJoinKey.Value<string>(SettingManager));
            IHtExpression fileKey = GetKeyFromExpression(FileFilterJoinKey.Value<string>(SettingManager));

            IEnumerator<IHtValue> toFilter = rows.GetEnumerator();
            IEnumerator<IHtValue> outerRows = PipelinePlugin.FromJsonStream(new FileStream(path, FileMode.Open), _jsonFactory, isGzip, false).GetEnumerator();            

            if (toFilter.MoveNext() && outerRows.MoveNext())
            {
                var leftKey = filteredKey.Evaluate(toFilter.Current);
                var rightKey = fileKey.Evaluate(outerRows.Current);
                while (true)
                {
                    var comparison = _comparer.Compare(leftKey, rightKey);
                    if (comparison == 0)
                    {
                        IHtValue rowResult = null;
                        switch (jt)
                        {
                            case JoinTypes.FilterOnly:
                                rowResult = toFilter.Current;
                                break;
                            case JoinTypes.AllColumns:
                                rowResult = new JsonHtValue(toFilter.Current);
                                var keys = rowResult.AsDictionaryElements.Select(e => e.Key).ToArray();
                                foreach (var kvp in outerRows.Current.AsDictionaryElements)
                                {
                                    var key = kvp.Key;
                                    var origKey = key;
                                    for (int i=0; true; i++)
                                    {
                                        if (keys.Contains(key))
                                        {
                                            key = $"{origKey}-{i}";
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                    rowResult[key] = kvp.Value;
                                }
                                break;
                            case JoinTypes.ProjectionExpression:
                                rowResult = new JsonHtValue();
                                rowResult["left"] = toFilter.Current;
                                rowResult["right"] = outerRows.Current;
                                rowResult = projectionExpression.Evaluate(rowResult);
                                break;
                        }
                        yield return rowResult;
                        if (jt == JoinTypes.FilterOnly || _comparer.Compare(rightKey, fileKey.Evaluate(outerRows.Current))!=0) {
                            if (! toFilter.MoveNext()) {
                                break;
                            }
                            leftKey=filteredKey.Evaluate(toFilter.Current);
                        } else if (outerRows.MoveNext()) {
                            rightKey=fileKey.Evaluate(outerRows.Current);
                        } else if (toFilter.MoveNext()) {
                            leftKey=filteredKey.Evaluate(toFilter.Current);
                        } else {
                            break;
                        }
                    }
                    else if (comparison < 0)
                    {
                        if (! toFilter.MoveNext())
                        {
                            break;
                        }
                    }
                    else
                    {
                        if (! outerRows.MoveNext())
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}


