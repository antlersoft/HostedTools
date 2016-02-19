using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class OperatorValidity
    {
        public OperatorValidity(Type resultType)
        {
            ResultType = resultType;
            IsValid = true;
            Message = String.Empty;
        }

        public OperatorValidity(string message)
        {
            IsValid = false;
            Message = message;
        }

        public Type ResultType { get; private set; }

        public string Message { get; private set; }

        public bool IsValid { get; private set; }
    }
}
