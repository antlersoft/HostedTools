using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public abstract class ParseRule
    {
        List<Symbol> _toMatch;

        public ParseRule(params Symbol[] toMatch)
        {
            _toMatch = new List<Symbol>(toMatch);
        }

        public abstract GoalResult Matched(Type dataType, List<GoalResult> subGoals);
        public GoalResult TryRule(ParseState ps)
        {
            int tokenOffset = ps.TokenOffset;
            List<GoalResult> result = new List<GoalResult>();
            foreach (Symbol s in _toMatch)
            {
                if (tokenOffset >= ps.Tokens.Count)
                {
                    return new GoalResult("Incomplete statement");
                }
                if (ps.Tokens[tokenOffset].Name == s)
                {
                    ps.Write("Accepted " + s.Name);
                    result.Add(
                        new GoalResult(
                            new TokenNode(ps.Tokens[tokenOffset]),
                            ++tokenOffset));
                }
                else
                {
                    IGoal goal;
                    if (ps.Goals.TryGetValue(s, out goal))
                    {
                        GoalResult r = goal.TryGoal(new ParseState(ps, tokenOffset));
                        if (r.Succeeded)
                        {
                            ps.Write("Recognized " + s.Name);
                            tokenOffset = r.NextTokenOffset;
                            result.Add(r);
                        }
                        else
                        {
                            ps.Write("Failed to recognize " + s.Name+" at "+tokenOffset+":"+ps.Tokens[tokenOffset].Value);
                            return r;
                        }
                    }
                    else
                    {
                        return new GoalResult("Failed to parse at "+ps.Tokens[tokenOffset].Value+" looking for "+s.Name);
                    }
                }
            }
            return Matched(ps.DataType, result);
        }
    }
}
