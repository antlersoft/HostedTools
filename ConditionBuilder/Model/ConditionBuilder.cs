using System.ComponentModel.Composition;
using System.IO;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model.Internal;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    [Export(typeof(IConditionBuilder))]
    public class ConditionBuilder : IConditionBuilder
    {
        readonly ConditionParser _parser = new ConditionParser();

        public IHtExpression ParseCondition(string expr)
        {
            if (string.IsNullOrEmpty(expr))
            {
                return null;
            }
            return (IHtExpression) _parser.ParseExpression(ConditionParser.ExprSym, typeof (bool), expr)(this);
        }

        public IHtExpression ParseConditionVerbose(string expr, TextWriter writer)
        {
            if (string.IsNullOrEmpty(expr))
            {
                return null;
            }
            return (IHtExpression)_parser.ParseExpression(ConditionParser.ExprSym, typeof(bool), expr, writer)(this);            
        }
    }
}
