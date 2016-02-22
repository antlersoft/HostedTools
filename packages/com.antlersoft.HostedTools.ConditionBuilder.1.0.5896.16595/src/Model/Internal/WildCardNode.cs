using System;

using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal class WildCardNode : IParseTreeNode
    {
        public Func<object, object> GetFunctor()
        {
            return d => new WildCardExpression();
        }

        public Type ResultType
        {
            get { throw new NotImplementedException(); }
        }
    }

    internal class WildCardExpression : IHtExpression
    {

        public IHtValue Evaluate(IHtValue data)
        {
            throw new NotImplementedException();
        }
    }
}
