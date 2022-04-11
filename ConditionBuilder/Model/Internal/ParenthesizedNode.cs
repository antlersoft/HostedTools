using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class ParenthesizedNode : IParseTreeNode
    {
        private readonly IParseTreeNode _wrapped;
        internal ParenthesizedNode(IParseTreeNode wrapped)
        {
            _wrapped = wrapped;
        }
        public Type ResultType => _wrapped.ResultType;

        public Func<object, object> GetFunctor()
        {
            return _wrapped.GetFunctor();
        }
    }
}
