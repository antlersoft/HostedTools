
namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class InFixGoal : IGoal
    {
        public GoalResult TryGoal(ParseState parseState)
        {
            GoalResult simpleResult = parseState.Goals[ParserBase.SimpleExprSym].TryGoal(parseState);
            if (!simpleResult.Succeeded)
            {
                return simpleResult;
            }
            if (simpleResult.NextTokenOffset < parseState.Tokens.Count &&
                parseState.Tokens[simpleResult.NextTokenOffset] is InFixOperator)
            {
                InFixOperator op = (InFixOperator)parseState.Tokens[simpleResult.NextTokenOffset];
                ParseState nextState = new ParseState(parseState, simpleResult.NextTokenOffset + 1);
                GoalResult exprResult = parseState.Goals[ParserBase.InFixExprSym].TryGoal(nextState);
                if (!exprResult.Succeeded)
                {
                    exprResult = parseState.Goals[ParserBase.SimpleExprSym].TryGoal(nextState);
                    if (!exprResult.Succeeded)
                    {
                        return exprResult;
                    }
                }
                // Do precedence
                InFixNode nextInfix = null;
                InFixNode topNode = exprResult.Node as InFixNode;
                for (InFixNode candidateInfix = topNode;
                    candidateInfix != null && candidateInfix.Operator.Precedence <= op.Precedence;
                    candidateInfix = candidateInfix.LeftNode as InFixNode)
                {
                    nextInfix = candidateInfix;
                }
                IParseTreeNode rightNode = exprResult.Node;
                if (nextInfix != null)
                {
                    rightNode = nextInfix.LeftNode;
                }
                InFixNode node = new InFixNode(null, op, simpleResult.Node, rightNode);
                // Put node in tree according to precedence
                if (nextInfix != null)
                {
                    nextInfix.LeftNode = node;
                }
                else
                {
                    topNode = node;
                }
                return new GoalResult(topNode, exprResult.NextTokenOffset);
            }
            return new GoalResult("Not an infix expression");
        }

        public Symbol Name
        {
            get { return ParserBase.InFixExprSym; }
        }
    }
}
