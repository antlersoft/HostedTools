using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class ValueComparer : IComparer<IHtValue>
    {
        private bool _nullsLast;
        private bool _descending;

        public ValueComparer(bool descending = false, bool nullsLast = false)
        {
            _descending = descending;
            _nullsLast = nullsLast;
        }

        public int Compare(IHtValue x, IHtValue y)
        {
            if (x.IsEmpty)
            {
                if (y.IsEmpty)
                {
                    return 0;
                }
                return _nullsLast ? 1 : -1;
            }
            if (y.IsEmpty)
            {
                return _nullsLast ? -1 : 1;
            }
            int diff;
            if (x.IsBool && y.IsBool)
            {
                diff = (x.AsBool ? 1 : 0) - (y.AsBool ? 1 : 0);
            }
            else if (x.IsLong && y.IsLong)
            {
                diff = x.AsLong > y.AsLong ? 1 : (y.AsLong > x.AsLong ? -1 : 0);
            }
            else if ((x.IsDouble || x.IsLong) && (y.IsDouble || y.IsLong))
            {
                diff = x.AsDouble > y.AsDouble ? 1 : (y.AsDouble > x.AsDouble ? -1 : 0);
            }
            else
            {
                diff = String.CompareOrdinal(x.AsString, y.AsString);
            }
            return _descending ? -diff : diff;
        }
    }

    public class OrderByComparer : IComparer<IHtValue>
    {
        private ValueComparer _values;
        private List<IHtExpression> _expressions;
        public OrderByComparer(ValueComparer values, List<IHtExpression> expressions)
        {
            _values = values;
            _expressions = expressions;
        }

        public int Compare(IHtValue x, IHtValue y)
        {
            foreach (var expr in _expressions)
            {
                int diff = _values.Compare(expr.Evaluate(x), expr.Evaluate(y));
                if (diff != 0)
                {
                    return diff;
                }
            }
            return 0;
        }
    }
}