using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

internal class InlineArrayNode : IParseTreeNode
{
    private IParseTreeNode _arguments;
    public Type ResultType => typeof(IHtExpression);

    internal InlineArrayNode(IParseTreeNode arguments) {
        _arguments = arguments;
    }

    public Func<object, object> GetFunctor()
    {
        return d => new OperatorExpression("@[]", l =>
        {
            var result = new JsonHtValue();
            int i = 0;
            foreach (var v in l)
            {
                result[i++] = v;
            }
            return result;
        }, (IList<IHtExpression>)_arguments.GetFunctor()(d));
    }
}