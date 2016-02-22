
namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public interface IGoal
    {
        GoalResult TryGoal(ParseState parseState);
        Symbol Name { get; }
    }
}
