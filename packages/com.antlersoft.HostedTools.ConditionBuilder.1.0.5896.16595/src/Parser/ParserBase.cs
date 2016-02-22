using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class ParserBase
    {
        public static readonly Symbol InFixExprSym = new Symbol("InFixExprSym");
        public static readonly Symbol SimpleExprSym = new Symbol("SimpleExprSym");
        private readonly Dictionary<Symbol, IGoal> _goalDictionary;
        private readonly List<NextToken> _matchers;

        public static int SkipWhitespace(String toParse, int currentIndex, int len)
        {
            for (; currentIndex < len && Char.IsWhiteSpace(toParse[currentIndex]); ++currentIndex)
            { }
            return currentIndex;
        }

        protected ParserBase(IEnumerable<NextToken> matchers, IEnumerable<IGoal> goals)
        {
            _matchers = matchers.ToList();
            _goalDictionary = goals.ToDictionary(g => g.Name);
        }

        public Func<object, object> ParseExpression(Symbol topGoal, Type dataType, string expression)
        {
            return ParseExpression(topGoal, dataType, expression, null);
        }

        public Func<object, object> ParseExpression(Symbol topGoal, Type dataType, string expression, TextWriter writer)
        {
            int len = expression.Length;
            List<Token> lexList = new List<Token>();
            bool foundTerm = true;
            for (int currentIndex = 0; foundTerm && currentIndex < len; )
            {
                currentIndex = SkipWhitespace(expression, currentIndex, len);
                if (currentIndex >= len)
                {
                    break;
                }
                foundTerm = false;
                foreach (var matcher in _matchers)
                {
                    if (matcher.next(expression, ref currentIndex, lexList))
                    {
                        foundTerm = true;
                        break;
                    }
                }
            }
            GoalResult result = _goalDictionary[topGoal].TryGoal(new ParseState(_goalDictionary, lexList, 0, dataType, writer));
            if (!result.Succeeded || result.NextTokenOffset != lexList.Count)
            {
                throw new ArgumentException("Failed to parse [" + expression + "] as an expression. " + result.FailureMessage);
            }
            return result.Node.GetFunctor();
        }
    }
}
