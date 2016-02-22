
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal class SubscriptGoal : IGoal
    {
        public Symbol Name
        {
            get { return ConditionParser.SubscriptExpressionSym; }
        }

        public GoalResult TryGoal(ParseState parseState)
        {
            GoalResult subscriptableResult =
                parseState.Goals[ConditionParser.SubscriptableExpressionSym].TryGoal(parseState);
            if (! subscriptableResult.Succeeded)
            {
                return subscriptableResult;
            }
            GoalResult currentResult = subscriptableResult;
            while (currentResult.NextTokenOffset < parseState.Tokens.Count &&
                   parseState.Tokens[currentResult.NextTokenOffset].Name == ConditionParser.LeftBracket)
            {
                ParseState nextState = new ParseState(parseState, currentResult.NextTokenOffset + 1);
                GoalResult exprResult = parseState.Goals[ConditionParser.ExprSym].TryGoal(nextState);
                if (!exprResult.Succeeded)
                {
                    return exprResult;
                }
                if (exprResult.NextTokenOffset < parseState.Tokens.Count &&
                    parseState.Tokens[exprResult.NextTokenOffset].Name == ConditionParser.RightBracket)
                {
                    GoalResult subscripted = currentResult;
                    currentResult =
                        new GoalResult(
                            new ParseTreeNode(typeof (IHtValue),
                                obj =>
                                    new SubscriptExpression((IHtExpression) subscripted.Node.GetFunctor()(obj),
                                        (IHtExpression) exprResult.Node.GetFunctor()(obj))),
                            exprResult.NextTokenOffset + 1);
                }
                else
                {
                    return new GoalResult("Missing right brace");
                }
            }
            return currentResult;
        }
    }
}
