using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class FunctionCallNode : IParseTreeNode
    {
        private IParseTreeNode _nameNode;
        private IParseTreeNode _argumentsNode;
        private IEnumerable<IFunctionSource> _sources;
        /// <summary>
        /// Start of UnixTime epoch; DateTimeKind is Utc
        /// </summary>
        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static Func<FunctionCallNode,string,Func<object,object>> MinMaxFunctor = (t, n) => (d => {
            var expressionList = (List<IHtExpression>)t._argumentsNode.GetFunctor()(d);
            if (expressionList.Count > 1) {
                throw new ArgumentException($"{n}() group method can only have one argument");
            }
            var expression = expressionList.Count == 1 ? expressionList[0] : null;
            return new MinMax(n == "min", expression);
        });

        internal FunctionCallNode(IEnumerable<IFunctionSource> functionSources, IParseTreeNode nameNode, IParseTreeNode argumentsNode)
        {
            _sources = functionSources;
            _nameNode = nameNode;
            _argumentsNode = argumentsNode;
        }

        static private IFunctionNamespace _builtIn = BuiltInFunctionSource._result[0];

        public Func<object, object> GetFunctor()
        {
            Func<FunctionCallNode,string,Func<object,object>> evaluator;
            FunctionIdNode idNode = (FunctionIdNode)_nameNode;
            IAddedGroupFunction groupFunc;
            IAddedFunction addedEvaluator;
            var name = idNode.Name;
            if (idNode.Namespace == null)
            {
                if (_builtIn.AddedGroupFunctions.TryGetValue(name, out groupFunc))
                {
                    return d => groupFunc.Functor.Invoke((IList<IHtExpression>)_argumentsNode.GetFunctor()(d));
                }
                if (_builtIn.AddedFunctions.TryGetValue(name, out addedEvaluator))
                {
                    return d => new OperatorExpression(name, addedEvaluator.Functor, (List<IHtExpression>)_argumentsNode.GetFunctor()(d));
                }
            }
            var namespaces = _sources.SelectMany(s => s.AvailableNamespaces);
            var namespacesToCheck = idNode.Namespace == null ? namespaces : namespaces.Where(n => n.Name == idNode.Namespace);
            foreach (var added in namespacesToCheck) {
                if (added.AddedGroupFunctions.TryGetValue(name, out groupFunc)) {
                    return d => groupFunc.Functor.Invoke((IList<IHtExpression>)_argumentsNode.GetFunctor()(d));
                }
                if (added.AddedFunctions.TryGetValue(name, out addedEvaluator)) {
                    return d => new OperatorExpression(name, addedEvaluator.Functor, (List<IHtExpression>) _argumentsNode.GetFunctor()(d));
                }
            }
            return d => new OperatorExpression(name, args => throw new InvalidOperationException($"No evaluator defined for function expression with name [{_nameNode.GetFunctor()(d)}]"),
                (IEnumerable<IHtExpression>)_argumentsNode.GetFunctor()(d));
        }

        public Type ResultType
        {
            get { return typeof(IHtExpression); }
        }
    }
}
