using System;

namespace com.antlersoft.HostedTools.Interface
{
    public interface ICredentialsFactory
    {
        IHtValue DefaultCredentials();
        IHtValue GetCredentials(string accountId, string password, string region = null);
    }

    public static class CredentialsFactoryConstants
    {
        public const string AccountId = "AccountId";
        public const string Password = "Password";
        public const string Region = "Region";
    }
}
