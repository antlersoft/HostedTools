
namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class GoalResult
    {
        public GoalResult(IParseTreeNode node, int offset)
        {
            NextTokenOffset = offset;
            Node = node;
            Succeeded = true;
            FailureMessage = string.Empty;
        }
        public GoalResult(string message)
        {
            Succeeded = false;
            FailureMessage = message;
        }
        public bool Succeeded { get; private set; }
        public IParseTreeNode Node { get; private set; }
        public int NextTokenOffset { get; private set; }
        public string FailureMessage { get; set; }
    }
}
