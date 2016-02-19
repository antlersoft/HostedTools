using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class BeginsWithExpression : OperatorExpression
    {
        public BeginsWithExpression(IHtExpression a, IHtExpression b)
            : base("BEGINS_WITH", f => new JsonHtValue(f[0].AsString.StartsWith(f[1].AsString)), new[] { a, b })
        {
            
        }
    }
}
