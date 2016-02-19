using System;
using System.Collections.Generic;

using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    internal abstract class ComparisonOperator : InFixOperator
    {
        internal ComparisonOperator(string op, int precedence)
            : base(op, precedence)
        {
            
        }

        public override object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand)
        {
            return new OperatorExpression(Name.Name, EvalIf,
                new[] { (IHtExpression)leftOperand(data), (IHtExpression)rightOperand(data) });
        }

        public override OperatorValidity GetReturnType(Type leftOperand, Type rightOperand)
        {
            return new OperatorValidity(typeof(bool));
        }

        internal IHtValue EvalIf(IList<IHtValue> args)
        {
            return new JsonHtValue(If(args));
        }

        internal bool If(IList<IHtValue> args)
        {
            if (args[0].IsBool)
            {
                if (args[0].AsBool)
                {
                    if (args[1].AsBool)
                    {
                        return FromComparisonValue(0);
                    }
                    return FromComparisonValue(1);
                }
                if (args[1].AsBool)
                {
                    return FromComparisonValue(-1);
                }
                return FromComparisonValue(0);
            }
            if (args[0].IsDouble)
            {
                return FromComparisonValue(args[0].AsDouble.CompareTo(args[1].AsDouble));
            }
            return
                FromComparisonValue(String.Compare(args[0].AsString, args[1].AsString, StringComparison.Ordinal));
        }
        internal abstract bool FromComparisonValue(int diff);
    }

    internal class EqualTo : ComparisonOperator
    {
        internal static readonly EqualTo I = new EqualTo();
        EqualTo()
            : base("==", 60)
        { }

        internal override bool FromComparisonValue(int diff)
        {
            return diff == 0;
        }
    }

    internal class NotEqualTo : ComparisonOperator
    {
        internal static readonly NotEqualTo I = new NotEqualTo();
        NotEqualTo()
            : base("!=", 60)
        { }

        internal override bool FromComparisonValue(int diff)
        {
            return diff != 0;
        }
    }

    internal class LessThanOrEqual : ComparisonOperator
    {
        internal static readonly LessThanOrEqual I = new LessThanOrEqual();
        LessThanOrEqual()
            : base("<=", 60)
        { }

        internal override bool FromComparisonValue(int diff)
        {
            return diff <= 0;
        }
    }
    internal class LessThan : ComparisonOperator
    {
        internal static readonly LessThan I = new LessThan();
        LessThan()
            : base("<", 60)
        { }

        internal override bool FromComparisonValue(int diff)
        {
            return diff < 0;
        }
    }
    internal class GreaterThan : ComparisonOperator
    {
        internal static readonly GreaterThan I = new GreaterThan();
        GreaterThan()
            : base(">", 60)
        { }

        internal override bool FromComparisonValue(int diff)
        {
            return diff > 0;
        }
    }

    internal class GreaterThanOrEqual : ComparisonOperator
    {
        internal static readonly GreaterThanOrEqual I = new GreaterThanOrEqual();
        GreaterThanOrEqual()
            : base(">=", 60)
        { }

        internal override bool FromComparisonValue(int diff)
        {
            return diff >= 0;
        }
    }
}
