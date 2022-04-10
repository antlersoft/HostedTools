using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class SubscriptExpression : IHtExpression
    {
        private IHtExpression _subscripted;
        private IHtExpression _subscript;

        public SubscriptExpression(IHtExpression subscripted, IHtExpression subscript)
        {
            _subscripted = subscripted;
            _subscript = subscript;
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
    }
}
