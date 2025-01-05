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

        private static readonly Dictionary<string, Func<FunctionCallNode,string,Func<object,object>>> AvailableFunctions = new Dictionary<string, Func<FunctionCallNode,string,Func<object,object>>>
        {
            {
                "split", (t,n) => t.GetFF(n, arg =>
                {
                    var result = new JsonHtValue();
                    int offset = 0;
                    foreach (string s in arg[0].AsString.Split(arg[1].AsString[0]))
                    {
                        result[offset++] = new JsonHtValue(s);
                    }
                    return result;
                })
            },
            {
                "deserialize", (t,n) => t.GetFF(n, arg => JsonConvert.DeserializeObject<IHtValue>(arg[0].AsString,new JsonFactory().GetSettings()))
            },
            {
                "serialize", (t,n) => t.GetFF(n, arg => new JsonHtValue(JsonConvert.SerializeObject(arg[0],new JsonFactory().GetSettings())))
            },
            {
                "count", (t,n) => (d => {
                    var argsList = (List<IHtExpression>)t._argumentsNode.GetFunctor()(d);
                    if (argsList.Count > 1)
                    {
                        throw new ArgumentException($"{n}() group method can only have one argument");
                    }
                    if (argsList.Count == 0) {
                        return new GroupCount();
                    }
                    return new GroupCountDistinct(argsList[0]);
                })
            },
            {
                "countdistinct", (t,n) => (d => new GroupCountDistinct(((List<IHtExpression>)t._argumentsNode.GetFunctor()(d))[0]))
            },
            {
                "sum", (t,n) => (d => new GroupSum(((List<IHtExpression>)t._argumentsNode.GetFunctor()(d))[0]))
            },
            {
                "ifthenelse", (t,n) => (d => new IfThenElseExpression((List<IHtExpression>) t._argumentsNode.GetFunctor()(d)))
            },
            {
                "min", MinMaxFunctor
            },
            {
                "max", MinMaxFunctor
            },
            {
                "abs", (t,n) => t.GetFF(n, arg => new JsonHtValue(Math.Abs(arg[0].AsDouble)))
            },
            {
                "round", (t,n) => t.GetFF(n, arg => new JsonHtValue(Math.Round(arg[0].AsDouble)))
            },
            {
                "floor", (t,n) => t.GetFF(n, arg => new JsonHtValue(Math.Floor(arg[0].AsDouble)))
            },
            {
                "ceiling", (t,n) => t.GetFF(n, arg => new JsonHtValue(Math.Ceiling(arg[0].AsDouble)))
            },
            {
                "truncate", (t,n) => t.GetFF(n, arg => new JsonHtValue(Math.Truncate(arg[0].AsDouble)))
            },
            {
                "datetime", (t,n) => t.GetFF(n, arg => new JsonHtValue((DateTime.UtcNow - Epoch).TotalSeconds))
            }
        };

        // Returns the functor for a function node that evaluates all its arguments
        internal Func<object,object> GetFF(string name, Func<IList<IHtValue>, IHtValue> evaluator)
        {
            return d => new OperatorExpression(name, evaluator, (List<IHtExpression>) _argumentsNode.GetFunctor()(d));
        }

        internal FunctionCallNode(IEnumerable<IFunctionSource> functionSources, IParseTreeNode nameNode, IParseTreeNode argumentsNode)
        {
            _sources = functionSources;
            _nameNode = nameNode;
            _argumentsNode = argumentsNode;
        }

        public Func<object, object> GetFunctor()
        {
            Func<FunctionCallNode,string,Func<object,object>> evaluator;
            FunctionIdNode idNode = (FunctionIdNode)_nameNode;
            var name = idNode.Name;
            if (! AvailableFunctions.TryGetValue(name, out evaluator))
            {
                var namespaces = _sources.SelectMany(s => s.AvailableNamespaces);
                var namespacesToCheck = idNode.Namespace == null ? namespaces : namespaces.Where(n => n.Name == idNode.Namespace);
                foreach (var added in namespacesToCheck) {
                    Func<IList<IHtExpression>,IGroupExpression> groupFunc;
                    if (added.AddedGroupFunctions.TryGetValue(name, out groupFunc)) {
                        return d => groupFunc.Invoke((IList<IHtExpression>)_argumentsNode.GetFunctor()(d));
                    }
                    Func<IList<IHtValue>, IHtValue> addedEvaluator;
                    if (added.AddedFunction.TryGetValue(name, out addedEvaluator)) {
                        return d => new OperatorExpression(name, addedEvaluator, (List<IHtExpression>) _argumentsNode.GetFunctor()(d));
                    }
                }
                return d => new OperatorExpression(name, args => throw new InvalidOperationException($"No evaluator defined for function expression with name [{_nameNode.GetFunctor()(d)}]"),
                    (IEnumerable<IHtExpression>)_argumentsNode.GetFunctor()(d));
            }
            return
                d =>
                    evaluator(this, name)(d);
        }

        public Type ResultType
        {
            get { return typeof(IHtExpression); }
        }
    }
}
