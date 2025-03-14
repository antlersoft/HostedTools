using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json.Linq;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class JoinTransform : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource, IAfterComposition, IHasSettingChangeActions
    {
        public enum JoinTypes {
            In,
            NotIn,
            JoinSymmetric,
            LeftJoin,
            RightJoin,
            RightNotIn,
            RightIn
        };
        public enum ResultTypes {
            FilterOnly,
            AllColumns,
            ProjectionExpression
        };
        /// <summary>
        /// This enumerator traverses an underlying enumerator assumed to be sorted on
        /// a sort key.  The client may indicate that a particular "Current" value is "Matched."
        /// If a value is Matched, it sets a ListMatched flag in the iterator.  If the sort key of the matched
        /// value does not match the sort key of the elements on the rewind list, the rewind list
        /// is cleared before the matched value is added to it; if it does match and the rewind list is not
        /// being replayed, it is added to the rewind list.
        /// 
        /// There is a RewindRepeatedKeys method that makes the first record in the rewind list the current if the
        /// rewind list is not empty abd ckears the ListMatched flag.
        /// (if the rewind list is empty, it does not change the state of the enumerator at all).  If rewound,
        /// the interator will step through the rewind list; while traversing the rewind list indicating a record
        /// is "Matched" sets the flag without changing the list.
        /// </summary>
        public class RewindWithSameKeyEnumerator : IEnumerator<IHtValue>
        {
            private IEnumerator<IHtValue> _underlying;
            private IHtExpression _keyExpression;
            private List<IHtValue> _rewindList = new List<IHtValue>();
            private int _rewindIndex = -1;
            private IHtValue _rewindKey = null;
            private IComparer<IHtValue> _comparer;
            private bool _isValidCurrent = false;
            private bool _rewindable = false;
            private bool _reachedEndOfUnderlying = false;

            public bool IsValidCurrent => _isValidCurrent;

            public RewindWithSameKeyEnumerator(IEnumerator<IHtValue> underlying, IHtExpression keyExpression, IComparer<IHtValue> comparer) {
                _underlying = underlying;
                _keyExpression = keyExpression;
                _comparer = comparer;
            }

            public void MakeRewindable() {
                _rewindable = true;
            }
            public IHtValue Current => (_rewindIndex >=0 && _rewindIndex < _rewindList.Count) ? _rewindList[_rewindIndex] : _underlying.Current;

            object IEnumerator.Current => (_rewindIndex >=0 && _rewindIndex < _rewindList.Count) ? _rewindList[_rewindIndex] : _underlying.Current;

            public IHtValue RewindKey => _rewindKey;

            public IHtValue CurrentKey => _keyExpression.Evaluate(Current);

            private IHtValue UnderyingCurrentKey => _keyExpression.Evaluate(_underlying.Current);
            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if (_rewindIndex >= 0) {
                    _rewindIndex++;
                    if (_rewindIndex >= _rewindList.Count) {
                        _rewindIndex = -1;
                        if (_reachedEndOfUnderlying) {
                            _isValidCurrent = false;
                            return false;
                        }
                    }
                    return true;
                }
                if (_reachedEndOfUnderlying) {
                    _isValidCurrent = false;
                    return false;
                }

                if (_isValidCurrent && _rewindable) {
                    if (_rewindKey == null) {
                        _rewindKey = UnderyingCurrentKey;
                    } else {
                        if (_comparer.Compare(CurrentKey, _rewindKey) != 0) {
                            _rewindList.Clear();
                            _rewindKey = UnderyingCurrentKey;
                        }
                    }
                    _rewindList.Add(_underlying.Current);
                }
                _isValidCurrent=_underlying.MoveNext();
                _reachedEndOfUnderlying = ! _isValidCurrent;
                return _isValidCurrent;
            }

            public void SkipToEnd() {
                _rewindIndex = -1;
                _reachedEndOfUnderlying = true;
                _isValidCurrent = false;
            }

            public void RewindRepeatedKeys() {
                if (! _rewindable) {
                    throw new InvalidOperationException("RewindRepeatedKeys called on non-rewindable enumerator");
                }
                if (_rewindList.Count > 0) {
                    _rewindIndex = 0;
                    _isValidCurrent = true;
                }
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }

        static ISettingDefinition JoinFile = new PathSettingDefinition("JoinFile", "Pipeline.JoinTransform", "Json data to load", false, false, "json file|*.json|gzip'd json|*.gz", "File containing json data to join with incoming rows");
        static readonly ISettingDefinition FileIsGzip = new SimpleSettingDefinition("FileIsGzip", "Pipeline.JoinTransform", "Join file is GZipped", "If checked, assumes json join file data is compressed in gzip format", typeof(bool), "false");
        static readonly ISettingDefinition UseSourceForFile = new SimpleSettingDefinition("UseSource", "Pipeline.JoinTransform", "Use source", "Use selected source plugin instead of file", typeof(bool), "false", false, 0);
        private readonly PluginSelectionSettingDefinition SourcePlugin;
        static ISettingDefinition ToFilterJoinKey = new SimpleSettingDefinition("InputJoinKey", "Pipeline.JoinTransform", "Key in input to join on", "Input must be sorted on given key");
        static ISettingDefinition FileFilterJoinKey = new SimpleSettingDefinition("FileJoinKey", "Pipeline.JoinTransform", "Key in file data to join on", "Data file must be sorted on given key");
        static ISettingDefinition JoinType = new SimpleSettingDefinition("JoinType", "Pipeline.JoinTransform", "Join Type", null, typeof(JoinTypes), "In", false, 0);
        static ISettingDefinition ResultType = new SimpleSettingDefinition("ResultType", "Pipeline.JoinTransform", "Result Type", null, typeof(ResultTypes), "FilterOnly", false, 0);
        static ISettingDefinition ProjectionExpression = new SimpleSettingDefinition("ProjectionExpression", "Pipeline.JoinTransform", "Projection Expression");

        [Import]
        public IConditionBuilder _expressionBuilder;
        [Import]
        public IJsonFactory _jsonFactory;

        private IComparer<IHtValue> _comparer = new ValueComparer();

        public JoinTransform()
        : base(new MenuItem("DevTools.Pipeline.Transform.JoinTransform", "Join Transform", typeof(JoinTransform).FullName, "DevTools.Pipeline.Transform"),
            new [] {JoinFile.FullKey(), FileIsGzip.FullKey(), UseSourceForFile.FullKey(), "Pipeline.JoinTransform.SourcePlugin", ToFilterJoinKey.FullKey(), FileFilterJoinKey.FullKey(), JoinType.FullKey(), ResultType.FullKey(), ProjectionExpression.FullKey()})
        {
             SourcePlugin = new PluginSelectionSettingDefinition(PipelinePlugin.NodeFunc<IRootNode>, "SourcePlugin", "Pipeline.JoinTransform", "Source to join with", "Select a source plugin for row data to join with");
             SourcePlugin.InjectImplementation(typeof(IReadOnly), new CanBeReadOnly());
        }

        public IEnumerable<ISettingDefinition> Definitions => new [] { JoinFile, FileIsGzip, UseSourceForFile, SourcePlugin, ToFilterJoinKey, FileFilterJoinKey, JoinType, ResultType, ProjectionExpression};

        public override string NodeDescription { get { string r= "unspecified join"; try {
            var scope = SettingManager.Scope(JoinType.ScopeKey);
            var setting = scope[JoinType.Name];
            var raw = setting.GetRaw();
            var type = JoinType.Type;
            object val = "[unspecified]";
            var joinWithText = UseSourceForFile.Value<bool>(SettingManager) ?
                ((PluginSelectionItem)SourcePlugin.FindMatchingItem(SourcePlugin.Value<string>(SettingManager))).Plugin.Cast<IRootNode>().NodeDescription
                : JoinFile.Value<string>(SettingManager);
            try {
                val = Enum.Parse(type, raw);
            } catch {}
            r=$"join {val} {joinWithText} "+
            (ResultType.Value<ResultTypes>(SettingManager) == ResultTypes.ProjectionExpression ? "Projection " + ProjectionExpression.Value<string>(SettingManager)
            : ResultType.Value<string>(SettingManager));} catch (Exception e) {} return r; } }

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey => new Dictionary<string, Action<IWorkMonitor, ISetting>> {
            {
                UseSourceForFile.FullKey(), (m,s) => { UpdateSourcePluginReadOnly(); }
            }
        };

        class Unchanged : IHtExpression
        {
            internal static Unchanged Instance = new Unchanged();
            public IHtValue Evaluate(IHtValue val)
            {
                return val;
            }
        }

        private void UpdateSourcePluginReadOnly() {
            (SourcePlugin.Cast<IReadOnly>() as CanBeReadOnly).ReadOnly = ! UseSourceForFile.Value<bool>(SettingManager);
        }

        private IHtExpression GetKeyFromExpression(string expr)
        {
            if (String.IsNullOrWhiteSpace(expr))
            {
                return Unchanged.Instance;
            }
            return _expressionBuilder.ParseCondition(expr);
        }

        public static bool UpdateStateForIn(bool bothValid, int comparison, RewindWithSameKeyEnumerator toFilter, RewindWithSameKeyEnumerator filter)       {
            bool returnRow = false;
            if (bothValid) {
                if (comparison == 0) {
                    returnRow = true;
                    toFilter.MoveNext();
                } else if (comparison < 0) {
                    toFilter.MoveNext();
                } else {
                    filter.MoveNext();
                }
            } else {
                if (toFilter.IsValidCurrent) {
                    toFilter.SkipToEnd();
                } else {
                    filter.SkipToEnd();
                }
            }
            return returnRow;
        }

        public static bool UpdateStateForNotIn(int comparison, RewindWithSameKeyEnumerator toFilter, RewindWithSameKeyEnumerator filter)
        {
            bool returnRow = false;
            if (toFilter.IsValidCurrent) {
                if (comparison < 0 || ! filter.IsValidCurrent) {
                    returnRow=true;
                    toFilter.MoveNext();
                } else if (comparison > 0) {
                    filter.MoveNext();
                } else {
                    toFilter.MoveNext();
                }
            } else {
                filter.SkipToEnd();
            }
            return returnRow;
        }

        public static bool UpdateStateForJoin(int comparison, bool bothValid, bool partial, IComparer<IHtValue> comparer, RewindWithSameKeyEnumerator toFilter, RewindWithSameKeyEnumerator filter, out bool partialJoin) {
            bool returnRow = false;
            bool partialStatus = false;

            if (bothValid) {
                if (comparison == 0) {
                    returnRow = true;
                    filter.MoveNext();
                    if (! filter.IsValidCurrent || (filter.RewindKey != null && comparer.Compare(filter.CurrentKey, filter.RewindKey)!=0)) {
                        filter.RewindRepeatedKeys();
                        toFilter.MoveNext();
                    }
                } else if (comparison < 0) {
                    if (partial && !(filter.IsValidCurrent && filter.RewindKey!=null && comparer.Compare(filter.RewindKey, toFilter.CurrentKey) == 0)) {
                        returnRow = true;
                        partialStatus = true;
                    }
                    toFilter.MoveNext();
                } else if (comparison > 0) {
                    filter.MoveNext();
                }
            } else {
                if (toFilter.IsValidCurrent) {
                    if (partial && ! (filter.RewindKey!=null && comparer.Compare(filter.RewindKey, toFilter.CurrentKey) == 0)) {
                        returnRow = true;
                        partialStatus = true;
                    }
                    if (! partial) {
                        toFilter.SkipToEnd();
                    } else {
                        toFilter.MoveNext();
                    }
                } else {
                    filter.SkipToEnd();
                }
            }
            partialJoin = partialStatus;
            return returnRow;
        }

        public static IHtValue OutputValue(JoinTypes jt, ResultTypes rt, IHtValue left, IHtValue right, IHtExpression projectionExpression, bool isPartial) {
            IHtValue result = null;
            IHtValue leftRow = null;
            IHtValue rightRow = null;
            switch (jt) {
                case JoinTypes.In:
                case JoinTypes.NotIn:
                    leftRow = left;
                    rightRow = new JsonHtValue();
                    break;
                case JoinTypes.RightIn:
                case JoinTypes.RightNotIn:
                    rightRow = right;
                    leftRow = new JsonHtValue();
                    break;
                case JoinTypes.JoinSymmetric:
                    leftRow = left;
                    rightRow = right;
                    break;
                case JoinTypes.LeftJoin:
                    leftRow = left;
                    rightRow = isPartial ? new JsonHtValue() : right;
                    break;
                case JoinTypes.RightJoin:
                    leftRow = isPartial ? new JsonHtValue() : left;
                    rightRow = right;
                    break;
            }
            switch (rt) {
                case ResultTypes.AllColumns:
                    result = new JsonHtValue(leftRow);
                    var keys = leftRow.IsDictionary ? result.AsDictionaryElements.Select(e => e.Key).ToArray() : new string[0];
                    if (rightRow.IsDictionary) {
                        foreach (var kvp in rightRow.AsDictionaryElements)
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
                            result[key] = kvp.Value;
                        }
                    }
                    break;
                case ResultTypes.FilterOnly:
                    switch (jt) {
                        case JoinTypes.In:
                        case JoinTypes.NotIn:
                        case JoinTypes.LeftJoin:
                        case JoinTypes.JoinSymmetric:
                            result = leftRow;
                            break;
                        case JoinTypes.RightJoin:
                        case JoinTypes.RightIn:
                        case JoinTypes.RightNotIn:
                            result = rightRow;
                            break;
                    }
                    break;
                case ResultTypes.ProjectionExpression:
                    result = new JsonHtValue();
                    result["left"] = leftRow;
                    result["right"] = rightRow;
                    result = projectionExpression.Evaluate(result);
                    break;
            }
            return result;
        }

        public static IEnumerable<IHtValue> GetJoin(JoinTypes jt, ResultTypes rt, IComparer<IHtValue> comparer, IHtExpression projectionExpression, IEnumerable<IHtValue> leftRows, IHtExpression leftKeyExp, IEnumerable<IHtValue> rightRows, IHtExpression rightKeyExp)
        {
            IHtValue leftKey = null;
            IHtValue rightKey = null;
            var leftRewind = new RewindWithSameKeyEnumerator(leftRows.GetEnumerator(), leftKeyExp, comparer);
            var rightRewind = new RewindWithSameKeyEnumerator(rightRows.GetEnumerator(), rightKeyExp, comparer);
            switch (jt) {
                case JoinTypes.LeftJoin:
                case JoinTypes.JoinSymmetric:
                    rightRewind.MakeRewindable();
                    break;
                case JoinTypes.RightJoin:
                    leftRewind.MakeRewindable();
                    break;
            }
            leftRewind.MoveNext();
            rightRewind.MoveNext();

            while (leftRewind.IsValidCurrent || rightRewind.IsValidCurrent)
            {
                leftKey = leftRewind.IsValidCurrent ? leftRewind.CurrentKey : null;
                rightKey = rightRewind.IsValidCurrent ? rightRewind.CurrentKey : null;
                var left = leftRewind.IsValidCurrent ? leftRewind.Current : null;
                var right = rightRewind.IsValidCurrent ? rightRewind.Current : null;
                bool bothValid = leftKey != null && rightKey != null;
                var comparison = bothValid ? comparer.Compare(leftKey, rightKey) : 0;
                bool partialRow;
                switch (jt) {
                    case JoinTypes.In:
                        if (UpdateStateForIn(bothValid, comparison, leftRewind, rightRewind)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, true);
                        }
                        break;
                    case JoinTypes.RightIn:
                        if (UpdateStateForIn(bothValid, -comparison, rightRewind, leftRewind)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, true);
                        }
                        break;
                    case JoinTypes.NotIn:
                        if (UpdateStateForNotIn(comparison, leftRewind, rightRewind)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, true);
                        }
                        break;
                    case JoinTypes.RightNotIn:
                        if (UpdateStateForNotIn(-comparison, rightRewind, leftRewind)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, true);
                        }
                        break;
                    case JoinTypes.LeftJoin:
                        if (UpdateStateForJoin(comparison, bothValid, true, comparer, leftRewind, rightRewind, out partialRow)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, partialRow);
                        }
                        break;
                    case JoinTypes.RightJoin:
                        if (UpdateStateForJoin(-comparison, bothValid, true, comparer, rightRewind, leftRewind, out partialRow)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, partialRow);
                        }
                        break;
                    case JoinTypes.JoinSymmetric:
                        if (UpdateStateForJoin(comparison, bothValid, false, comparer, leftRewind, rightRewind, out partialRow)) {
                            yield return OutputValue(jt, rt, left, right, projectionExpression, partialRow);
                        }
                        break;
                }
            }
        }

        class Transform : HostedObjectBase, IHtValueTransform, IDisposable {
            private readonly IComparer<IHtValue> _comparer;
            private readonly JoinTypes jt;
            private readonly ResultTypes rt;
            private readonly IHtExpression projectionExpression;
            private readonly IHtExpression filteredKey;
            private readonly IHtExpression fileKey;
            private IHtValueSource fileSource;

            internal Transform(IComparer<IHtValue> comparer, IJsonFactory jsonFactory, IHtValueSource aFileSource,
                JoinTypes _jt, ResultTypes _rt, IHtExpression _projectExpression, IHtExpression _filteredKey,
                IHtExpression _fileKey) {
                _comparer = comparer;
                jt = _jt;
                rt = _rt;
                projectionExpression = _projectExpression;
                filteredKey = _filteredKey;
                fileKey = _fileKey;
                fileSource = aFileSource;
            }

            public void Dispose()
            {
                if (fileSource.Cast<IDisposable>() is IDisposable disposable) {
                    disposable.Dispose();
                }
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
            {
                return GetJoin(jt, rt, _comparer, projectionExpression, rows, filteredKey, fileSource.GetRows(monitor), fileKey);
            }
        }

        private static bool StateUseSource(PluginState state) {
            string val;
            if (! state.SettingValues.TryGetValue(UseSourceForFile.FullKey(), out val)) {
                return false;
            }
            return (bool)Convert.ChangeType(val, typeof(bool));
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            string path = state.SettingValues[JoinFile.FullKey()];
            bool isGzip = (bool)Convert.ChangeType(state.SettingValues[FileIsGzip.FullKey()], typeof(bool));
            bool useSourcePlugin = StateUseSource(state);
            JoinTypes jt =Enum.Parse<JoinTypes>(state.SettingValues[JoinType.FullKey()]);
            ResultTypes rt =Enum.Parse<ResultTypes>(state.SettingValues[ResultType.FullKey()]);
            IHtExpression projectionExpression = null;
            if (rt == ResultTypes.ProjectionExpression)
            {
                projectionExpression = _expressionBuilder.ParseCondition(state.SettingValues[ProjectionExpression.FullKey()]);
            }
            
            IHtExpression filteredKey = GetKeyFromExpression(state.SettingValues[ToFilterJoinKey.FullKey()]);
            IHtExpression fileKey = GetKeyFromExpression(state.SettingValues[FileFilterJoinKey.FullKey()]);

            IHtValueSource fileSource = useSourcePlugin ?
                PluginManager[state.SettingValues[SourcePlugin.FullKey()]].Cast<IHtValueRoot>().GetHtValueSource(state.NestedValues[SourcePlugin.FullKey()])
                : PipelinePlugin.FromJsonStream(new FileStream(path, FileMode.Open, FileAccess.Read), _jsonFactory, isGzip, false);
            return new Transform(_comparer, _jsonFactory, fileSource, jt, rt,
                projectionExpression, filteredKey, fileKey);
        }

        public void AfterComposition()
        {
            SourcePlugin.SetPlugins(PluginManager.Plugins.Where(p => p.Cast<IRootNode>() != null).ToList(), SettingManager);
            UpdateSourcePluginReadOnly();
        }
    }
}


