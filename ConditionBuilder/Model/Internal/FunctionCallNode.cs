using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Start of UnixTime epoch; DateTimeKind is Utc
        /// </summary>
        public static DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static readonly Dictionary<string, Func<IList<IHtValue>, IHtValue>> AvailableFunctions = new Dictionary<string, Func<IList<IHtValue>, IHtValue>>
        {
            {
                "split", arg =>
                {
                    var result = new JsonHtValue();
                    int offset = 0;
                    foreach (string s in arg[0].AsString.Split(arg[1].AsString[0]))
                    {
                        result[offset++] = new JsonHtValue(s);
                    }
                    return result;
                }
            },
            {
                "deserialize", arg => JsonConvert.DeserializeObject<IHtValue>(arg[0].AsString,new JsonFactory().GetSettings())
            },
            {
                "serialize", arg => new JsonHtValue(JsonConvert.SerializeObject(arg[0],new JsonFactory().GetSettings()))
            },
            {
                "ifthenelse", arg => arg[0].AsBool ? arg[1] : arg[2]
            },
            {
                "truncate", arg => new JsonHtValue(Math.Truncate(arg[0].AsDouble))
            },
            {
                "datetime", arg => new JsonHtValue((DateTime.UtcNow - Epoch).TotalSeconds)
            }
        };

        internal FunctionCallNode(IParseTreeNode nameNode, IParseTreeNode argumentsNode)
        {
            _nameNode = nameNode;
            _argumentsNode = argumentsNode;
        }

        public Func<object, object> GetFunctor()
        {
            Func<IList<IHtValue>, IHtValue> evaluator;
            var name = ((TokenNode)_nameNode).Token.Value;
            if (! AvailableFunctions.TryGetValue(name, out evaluator))
            {
                evaluator = args => throw new InvalidOperationException($"No evaluator defined for function expression with name [{name}]");
            }
            return
                d =>
                    new OperatorExpression(name, evaluator,
                        (List<IHtExpression>) _argumentsNode.GetFunctor()(d));
        }

        public Type ResultType
        {
            get { return typeof(IHtExpression); }
        }
    }
}
