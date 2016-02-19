using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal class RegExpMatch : InFixOperator
    {
        internal RegExpMatch()
            : base("~=", 60)
        {
            
        }

        internal static readonly RegExpMatch I = new RegExpMatch();

        public override object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand)
        {
            return new RegExpExpression((IHtExpression) leftOperand(data), (IHtExpression) rightOperand(data));
        }

        public override OperatorValidity GetReturnType(Type leftOperand, Type rightOperand)
        {
            return new OperatorValidity(typeof(bool));
        }
    }

    internal class RegExpExpression : IOperatorExpression
    {
        private Regex _regex;
        private List<IHtExpression> _operands;

        internal RegExpExpression(IHtExpression data, IHtExpression regExp)
        {
            if (regExp is IConstantExpression)
            {
                _regex = new Regex(regExp.Evaluate(null).AsString);
            }
            _operands = new List<IHtExpression> {data, regExp};
        }

        public string OperatorName
        {
            get { return "~="; }
        }

        public IList<IHtExpression> Operands
        {
            get { return _operands; }
        }

        public IHtValue Evaluate(IHtValue data)
        {
            Regex regex = _regex ?? new Regex(_operands[1].Evaluate(data).AsString);
            return new JsonHtValue(regex.IsMatch(_operands[0].Evaluate(data).AsString ?? string.Empty));
        }
    }
}
