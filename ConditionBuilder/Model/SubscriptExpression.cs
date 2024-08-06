using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class SubscriptExpression : IHtExpression, IGroupExpression
    {
        private IHtExpression _subscripted;
        private IHtExpression _subscript;

        public SubscriptExpression(IHtExpression subscripted, IHtExpression subscript)
        {
            _subscripted = subscripted;
            _subscript = subscript;
        }

        public void AddRow(IHtValue row)
        {
            if (_subscripted is IGroupExpression group)
            {
                group.AddRow(row);
            }
            if (_subscript is IGroupExpression group2)
            {
                group2.AddRow(row);
            }
        }

        public void EndGroup()
        {
            if (_subscripted is IGroupExpression group)
            {
                group.EndGroup();
            }
            if (_subscript is IGroupExpression group2)
            {
                group2.EndGroup();
            }
        }

        public IHtValue Evaluate(IHtValue data)
        {
            IHtValue subscript = _subscript.Evaluate(data);
            IHtValue subscripted = _subscripted.Evaluate(data);
            IHtValue result;
            if (subscript.IsDouble)
            {
                result = subscripted[(int)subscript.AsDouble];
            }
            else if (subscript.IsLong)
            {
                result = subscripted[(int)subscript.AsLong];
            }
            else
            {
                result = subscripted[subscript.AsString];
            }
            if (result == null)
            {
                result = new JsonHtValue();
            }
            return result;
        }

        public void StartGroup()
        {
            if (_subscripted is IGroupExpression group)
            {
                group.StartGroup();
            }
            if (_subscript is IGroupExpression group2)
            {
                group2.StartGroup();
            }   
        }
    }
}
