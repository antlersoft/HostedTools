using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface
{
    public interface IConditionBuilder : IHostedObject
    {
        IHtExpression ParseCondition(string expr);
    }
}
