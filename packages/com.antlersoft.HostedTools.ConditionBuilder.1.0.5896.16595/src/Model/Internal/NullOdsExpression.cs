using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class NullHtExpression : IHtExpression
    {
        public IHtValue Evaluate(IHtValue data)
        {
            return new JsonHtValue();
        }
    }
}
