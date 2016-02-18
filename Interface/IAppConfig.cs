using System;
namespace com.antlersoft.HostedTools.Interface
{
    /// <summary>
    /// Get configuration values based on a string key
    /// </summary>
    public interface IAppConfig
    {
        string Get(string key, string def = null);

        int Get(string key, int def = 0);

        long Get(string key, long def = 0);

        double Get(string key, double def = 0);

        string NoDefault(string key);

        T NoDefault<T>(string key);

        T Get<T>(string key, T def = default(T));
    }
}

