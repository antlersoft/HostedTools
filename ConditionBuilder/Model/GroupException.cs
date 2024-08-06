using System;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class GroupException : System.Exception
    {
        public GroupException(string message, GroupFunctionBase target) : base(message + " in " + target.GetType().Name+" state = "+target.State.ToString())
        {
        }
    }
}