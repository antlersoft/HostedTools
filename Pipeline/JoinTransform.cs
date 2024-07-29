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
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json.Linq;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueTransform))]
    [Export(typeof(ISettingDefinitionSource))]
    public class JoinTransform : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource
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
            private bool _reachedEndOfUnderlying = false;

            public bool IsValidCurrent => _isValidCurrent;

            public RewindWithSameKeyEnumerator(IEnumerator<IHtValue> underlying, IHtExpression keyExpression, IComparer<IHtValue> comparer) {
                _underlying = underlying;
                _keyExpression = keyExpression;
                _comparer = comparer;
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

                if (_isValidCurrent) {
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
            new [] {Settings.LoadFile.FullKey(), Settings.GZipData.FullKey(), ToFilterJoinKey.FullKey(), FileFilterJoinKey.FullKey(), JoinType.FullKey(), ResultType.FullKey(), ProjectionExpression.FullKey()})
        {

        }

        public IEnumerable<ISettingDefinition> Definitions => new [] { ToFilterJoinKey, FileFilterJoinKey, JoinType, ResultType, ProjectionExpression};

        public string TransformDescription { get { string r= ""; try {
            var scope = SettingManager.Scope(JoinType.ScopeKey);
            var setting = scope[JoinType.Name];
            var raw = setting.GetRaw();
            var type = JoinType.Type;
            var val = Enum.Parse(type, raw);
            r=$"join {JoinType.Value<string>(SettingManager)} {Settings.LoadFile.Value<string>(SettingManager)} "+
            (ResultType.Value<ResultTypes>(SettingManager) == ResultTypes.ProjectionExpression ? "Projection " + ProjectionExpression.Value<string>(SettingManager)
            : ResultType.Value<string>(SettingManager));} catch (Exception e) {} return r; } }

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
                    if (partial && !(filter.IsValidCurrent && comparer.Compare(filter.RewindKey, toFilter.CurrentKey) == 0)) {
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
                    var keys = result.AsDictionaryElements.Select(e => e.Key).ToArray();
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
            leftRewind.MoveNext();
            var rightRewind = new RewindWithSameKeyEnumerator(rightRows.GetEnumerator(), rightKeyExp, comparer);
            rightRewind.MoveNext();

            while (leftRewind.IsValidCurrent || rightRewind.IsValidCurrent)
            {
                leftKey = leftRewind.IsValidCurrent ? leftRewind.CurrentKey : null;
                rightKey = rightRewind.IsValidCurrent ? rightRewind.CurrentKey : null;
                var left = leftRewind.Current;
                var right = rightRewind.Current;
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

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> rows, IWorkMonitor monitor)
        {
            string path = Settings.LoadFile.Value<string>(SettingManager);
            bool isGzip = Settings.GZipData.Value<bool>(SettingManager);
            JoinTypes jt = JoinType.Value<JoinTypes>(SettingManager);
            ResultTypes rt = ResultType.Value<ResultTypes>(SettingManager);
            IHtExpression projectionExpression = null;
            if (rt == ResultTypes.ProjectionExpression)
            {
                projectionExpression = _expressionBuilder.ParseCondition(ProjectionExpression.Value<string>(SettingManager));
            }
            
            IHtExpression filteredKey = GetKeyFromExpression(ToFilterJoinKey.Value<string>(SettingManager));
            IHtExpression fileKey = GetKeyFromExpression(FileFilterJoinKey.Value<string>(SettingManager));

            return GetJoin(jt, rt, _comparer, projectionExpression, rows, filteredKey, PipelinePlugin.FromJsonStream(new FileStream(path, FileMode.Open, FileAccess.Read), _jsonFactory, isGzip, false), fileKey);
        }
    }
}


