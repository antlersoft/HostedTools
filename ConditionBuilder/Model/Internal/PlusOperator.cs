using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class PlusOperator : InFixOperator
    {
        internal static readonly PlusOperator I = new PlusOperator();
        private PlusOperator()
            : base("+", 100)
        {
            
        }
        public override object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand)
        {
            return new OperatorExpression("+", arg =>
            {
                if (arg[0].IsDouble || arg[1].IsDouble)
                {
                    try
                    {
                        return new JsonHtValue(arg[0].AsDouble + arg[1].AsDouble);
                    }
                    catch (FormatException)
                    {
                    }
                }
                return new JsonHtValue(arg[0].AsString + arg[1].AsString);
            },
            new List<IHtExpression> { (IHtExpression)leftOperand(data), (IHtExpression)rightOperand(data) });
        }

        public override OperatorValidity GetReturnType(Type leftOperand, Type rightOperand)
        {
            if (ViableType(leftOperand) && ViableType(rightOperand))
            {
                return new OperatorValidity(typeof(IHtExpression));
            }
            return new OperatorValidity("+ must combine IHtExpression values");
        }

        private bool ViableType(Type operand)
        {
            return operand == typeof (IHtExpression) || operand == typeof(IHtValue) || operand == typeof (double) || operand == typeof (string);
        }
    }
}
