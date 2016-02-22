using System;
using System.Collections.Generic;


namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class FunctorParseRule : ParseRule
    {
        Func<Type, List<GoalResult>, GoalResult> _functor;
        public FunctorParseRule(Func<Type, List<GoalResult>, GoalResult> functor, params Symbol[] toMatch)
            : base(toMatch)
        {
            _functor = functor;
        }
        public override GoalResult Matched(Type dataType, List<GoalResult> subGoals)
        {
            return _functor(dataType, subGoals);
        }
    }
}
