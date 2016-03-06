using System;


namespace com.antlersoft.HostedTools.Framework.Model.Navigation
{
    public class RejectNavigationException : Exception
    {
        public RejectNavigationException(string message = null, Exception innerException = null)
            : base(message ?? "Bad navigation", innerException)
        {

        }
    }
}
