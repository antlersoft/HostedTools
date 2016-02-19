using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal abstract class MathOperator : InFixOperator
    {
        internal MathOperator(string name, int precedence)
            : base(name, precedence)
        {
            
        }

        public override object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand)
        {
            return new OperatorExpression("+", arg => new JsonHtValue(NumericEvaluate(arg[0].AsDouble, arg[1].AsDouble)),
            new List<IHtExpression> { (IHtExpression)leftOperand(data), (IHtExpression)rightOperand(data) });
        }

        protected abstract double NumericEvaluate(double d1, double d2);

        public override OperatorValidity GetReturnType(Type leftOperand, Type rightOperand)
        {
            if (ViableType(leftOperand) && ViableType(rightOperand))
            {
                return new OperatorValidity(typeof(IHtExpression));
            }
            return new OperatorValidity("+ must combine numeric IHtExpression values");
        }

        private bool ViableType(Type operand)
        {
            return operand == typeof(IHtExpression) || operand == typeof(IHtValue) || operand == typeof(double);
        }
    }

    internal class TimesOperator : MathOperator
    {
        internal static readonly TimesOperator I = new TimesOperator();
        internal TimesOperator()
            : base("*", 150)
        {
            
        }

        protected override double NumericEvaluate(double d1, double d2)
        {
            return d1*d2;
        }
    }

    internal class DivideOperator : MathOperator
    {
        internal static readonly DivideOperator I = new DivideOperator();
        internal DivideOperator()
            : base("/", 150)
        {

        }

        protected override double NumericEvaluate(double d1, double d2)
        {
            return d1 / d2;
        }
    }
    internal class SubtractOperator : MathOperator
    {
        internal static readonly SubtractOperator I = new SubtractOperator();
        internal SubtractOperator()
            : base("-", 100)
        {

        }

        protected override double NumericEvaluate(double d1, double d2)
        {
            return d1 - d2;
        }
    }

}
