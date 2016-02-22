using System;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class ProjectionItemNode : IParseTreeNode
    {
        internal IParseTreeNode NameNode;
        internal IParseTreeNode ValueNode;

        internal ProjectionItemNode(IParseTreeNode nameNode, IParseTreeNode valueNode)
        {
            NameNode = nameNode;
            ValueNode = valueNode;
        }

        public Func<object, object> GetFunctor()
        {
            return null;
        }

        public Type ResultType
        {
            get { return typeof(ProjectionItemNode); }
        }
    }
}
