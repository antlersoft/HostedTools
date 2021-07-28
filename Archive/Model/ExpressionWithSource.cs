using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model
{
    public class ExpressionWithSource : HostedObjectBase, IHtExpressionWithSource
    {
        private string _source;
        private IHtExpression _underlying;
        public ExpressionWithSource(IConditionBuilder builder, string expressionSource)
        {
            _underlying = builder.ParseCondition(expressionSource);
            _source = expressionSource;
        }

        public ExpressionWithSource(IConditionBuilder builder, string expressionSource, TextWriter writer)
        {
            _underlying = ((com.antlersoft.HostedTools.ConditionBuilder.Model.ConditionBuilder)builder).ParseConditionVerbose(expressionSource, writer);
            _source = expressionSource;
        }

        public IHtExpression Underlying => _underlying;

        public string ExpressionSource => _source;

        public IHtValue Evaluate(IHtValue data)
        {
            return _underlying.Evaluate(data);
        }
    }
}
