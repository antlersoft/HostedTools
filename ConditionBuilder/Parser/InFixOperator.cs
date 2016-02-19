using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public abstract class InFixOperator : Token
    {
        protected InFixOperator(string op, int precedence)
            : base(op)
        {
            Precedence = precedence;
        }

        public int Precedence { get; private set; }

        public abstract OperatorValidity GetReturnType(Type leftOperand, Type rightOperand);

        public abstract object Evaluate(object data, Func<object, object> leftOperand, Func<object, object> rightOperand);
    }
}
