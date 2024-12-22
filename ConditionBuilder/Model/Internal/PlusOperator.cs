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

        private bool CoerceToDouble(IHtValue o, out double result)
        {
            if (o.IsDouble)
            {
                result = o.AsDouble;
                return true;
            }
            if (o.IsLong)
            {
                result = o.AsLong;
                return true;
            }
            return double.TryParse(o.AsString, out result);
        }

        private bool CoerceToLong(IHtValue o, out long result)
        {
            if (o.IsLong)
            {
                result = o.AsLong;
                return true;
            }
            return long.TryParse(o.AsString, out result);
        }

        public override object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand)
        {
            return new OperatorExpression("+", arg =>
            {
                if (arg[0].IsDouble)
                {
                    double a1;
                    if (CoerceToDouble(arg[1], out a1))
                    {
                        return new JsonHtValue(arg[0].AsDouble + a1);
                    }
                }
                else if (arg[1].IsDouble)
                {
                    double a1;
                    if (CoerceToDouble(arg[0], out a1))
                    {
                        return new JsonHtValue(arg[1].AsDouble + a1);
                    }
                }
                else if (arg[0].IsLong)
                {
                    long a1;
                    if (CoerceToLong(arg[1], out a1))
                    {
                        return new JsonHtValue(arg[0].AsLong + a1);
                    }
                }
                else if (arg[1].IsLong)
                {
                    long a1;
                    if (CoerceToLong(arg[0], out a1))
                    {
                        return new JsonHtValue(arg[1].AsLong + a1);
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
