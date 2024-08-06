using System.Collections.Generic;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    public class GroupCountDistinct : GroupFunctionBase
    {
        private IHtExpression _expression;
        private SortedDictionary<IHtValue,IHtValue> _distinctValues;

        public GroupCountDistinct(IHtExpression expression=null, IComparer<IHtValue> comparer=null)
        {
            _expression = expression;
            if (comparer==null)
            {
                comparer = new ValueComparer();
            }
            _distinctValues = new SortedDictionary<IHtValue, IHtValue>();
        }

        public override void StartGroup()
        {
            base.StartGroup();
            _distinctValues.Clear();
        }

        public override void AddRow(IHtValue row)
        {
            base.AddRow(row);
            if (_expression != null)
            {
                row = _expression.Evaluate(row);
            }
            _distinctValues[row] = row;
        }

        public override IHtValue Evaluate(IHtValue row)
        {
            if (State != GroupFunctionState.End)
            {
                throw new GroupException("GroupCountDistinct.Evaluate called before group ended", this);
            }
            return new JsonHtValue(_distinctValues.Count);
        }
    }
}