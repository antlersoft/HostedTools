using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface
{
    public interface IGroupExpression : IHtExpression
    {
        void StartGroup();
        void AddRow(IHtValue row);
        void EndGroup();
    }
}