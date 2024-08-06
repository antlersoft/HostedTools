using System.Collections.Generic;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class MinMax : GroupFunctionBase
    {
        private readonly bool _isMin;
        private IHtValue _currentValue;
        private IHtExpression _expression;
        private IComparer<IHtValue> _comparer;

        public MinMax(bool isMin, IHtExpression expression, IComparer<IHtValue> comparer=null)
        {
            _expression = expression;
            _isMin = isMin;
            _comparer = comparer??new ValueComparer();
        }

        public override IHtValue Evaluate(IHtValue data)
        {
            if (State != GroupFunctionState.End)
            {
                throw new GroupException("MinMax.Evaluate called before group ended", this);
            }
            return _currentValue;
        }

        public override void StartGroup()
        {
            base.StartGroup();
            _currentValue = new JsonHtValue();
        }

        public override void AddRow(IHtValue row)
        {
            base.AddRow(row);
            if (_expression != null) {
                row = _expression.Evaluate(row);
            }
            if (_currentValue.IsEmpty)
            {
                _currentValue = row;
            }
            else
            {
                if (_isMin)
                {
                    if (_comparer.Compare(row, _currentValue) < 0)
                    {
                        _currentValue = row;
                    }
                }
                else
                {
                    if (_comparer.Compare(row, _currentValue) > 0)
                    {
                        _currentValue = row;
                    }
                }
            }
        }
    }
}