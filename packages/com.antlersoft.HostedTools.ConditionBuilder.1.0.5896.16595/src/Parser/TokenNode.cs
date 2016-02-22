using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class TokenNode : IParseTreeNode
    {
        Token _t;
        public TokenNode(Token t)
        {
            _t = t;
        }

        public Type ResultType
        {
            get { return typeof(string); }
        }

        public Func<object, object> GetFunctor()
        {
            return null;
        }

        public Token Token
        {
            get { return _t; }
        }
    }
}
