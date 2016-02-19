using com.antlersoft.HostedTools.Interface;

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
            if (subscript.IsDouble)
            {
                return subscripted[(int) subscript.AsDouble];
            }
            return subscripted[subscript.AsString];
        }
    }
}
