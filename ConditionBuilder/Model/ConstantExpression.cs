using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    public class ConstantExpression : IConstantExpression
    {
        private readonly IHtValue _value;

        public ConstantExpression(IHtValue val)
        {
            _value = val;
        }

        public IHtValue Evaluate(IHtValue data)
        {
            return _value;
        }
    }
}
