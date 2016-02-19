using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class SG : IGoal
    {
        private List<ParseRule> _rules;

        public SG(Symbol symbol, params ParseRule[] rules)
        {
            Name = symbol;
            _rules = new List<ParseRule>(rules);
        }

        public GoalResult TryGoal(ParseState parseState)
        {
            GoalResult gr = null;
            String failureMessage = String.Empty;
            foreach (var rule in _rules)
            {
                gr = rule.TryRule(parseState);
                if (gr.Succeeded)
                {
                    failureMessage = string.Empty;
                    break;
                }
                else
                {
                    failureMessage = failureMessage + gr.FailureMessage;
                }
            }
            gr.FailureMessage = failureMessage;
            return gr;
        }

        public Symbol Name
        {
            get;
            private set;
        }
    }
}
