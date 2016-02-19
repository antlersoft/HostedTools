using System;
using System.Collections.Generic;

using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal abstract class LogicalInFix : InFixOperator
    {
        internal LogicalInFix(string name, int precedence)
            : base(name, precedence)
        {

        }

        public override object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand)
        {
            return new LogicalOperatorExpression(this,
                new[] { (IHtExpression)leftOperand(data), (IHtExpression)rightOperand(data) });
        }

        public override OperatorValidity GetReturnType(Type leftOperand, Type rightOperand)
        {
            if ((leftOperand == typeof(bool) || typeof(IHtValue).IsAssignableFrom(leftOperand))&& (rightOperand == typeof(bool) || typeof(IHtValue).IsAssignableFrom(leftOperand)))
            {
                return new OperatorValidity(typeof(bool));
            }
            return new OperatorValidity("Logical operator applied to non-boolean operands");
        }

        internal abstract bool ShortCircuit(bool first, out bool result);

        internal abstract bool Evaluate(bool first, bool second);
    }

    internal class LogicalOperatorExpression : OperatorExpression
    {
        private LogicalInFix _op;
        internal LogicalOperatorExpression(LogicalInFix op,
            IEnumerable<IHtExpression> args)
            : base(op.Name.Name, null, args)
        {
            _op = op;
        }

        public override IHtValue Evaluate(IHtValue data)
        {
            bool first = Operands[0].Evaluate(data).AsBool;
            bool result;
            if (!_op.ShortCircuit(first, out result))
            {
                result = _op.Evaluate(first, Operands[1].Evaluate(data).AsBool);
            }
            return new JsonHtValue(result);
        }
    }

    class LogicalAnd : LogicalInFix
    {
        internal LogicalAnd()
            : base("&&", 25)
        {

        }

        internal override bool ShortCircuit(bool first, out bool result)
        {
            result = false;
            return !first;
        }

        internal override bool Evaluate(bool first, bool second)
        {
            return first && second;
        }
    }

    class LogicalOr : LogicalInFix
    {
        internal LogicalOr()
            : base("||", 20)
        {

        }

        internal override bool ShortCircuit(bool first, out bool result)
        {
            result = true;
            return first;
        }

        internal override bool Evaluate(bool first, bool second)
        {
            return first || second;
        }
    }
}
