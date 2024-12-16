using System;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal {
    internal class FunctionIdNode : IParseTreeNode
    {
        private string _name;
        private string _namespace;

        internal FunctionIdNode(IParseTreeNode node) {
            _name = ((TokenNode)node).Token.Value;
        }

        internal FunctionIdNode(IParseTreeNode node1, IParseTreeNode node2) {
            FunctionIdNode suffix = (FunctionIdNode)node2;
            _name = suffix._name;
            _namespace = ((TokenNode)node1).Token.Value;
            if (suffix._namespace != null) {
                _namespace = $"{_namespace}@{suffix._namespace}";
            }
        }

        internal string Name => _name;
        internal string Namespace => _namespace;
        public Type ResultType => typeof(string);

        public Func<object, object> GetFunctor()
        {
            return d => _namespace == null ? _name : $"{_namespace}@{_name}";
        }
    }
}