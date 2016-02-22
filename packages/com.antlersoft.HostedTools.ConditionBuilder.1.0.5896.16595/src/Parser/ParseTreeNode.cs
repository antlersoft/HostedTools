using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class ParseTreeNode : IParseTreeNode
    {
        private Func<object, object> _functor;

        public ParseTreeNode(Type rt, Func<object, object> functor)
        {
            ResultType = rt;
            _functor = functor;
        }

        public Type ResultType
        {
            get;
            private set;
        }

        public Func<object, object> GetFunctor()
        {
            return _functor;
        }
    }
}
