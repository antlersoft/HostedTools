using System;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Interface;
using Microsoft.Extensions.Configuration;

namespace com.antlersoft.HostedTools.Utility
{
    public abstract class AppConfigBase : IAppConfig
    {
        public virtual string Get(string key, string def = null)
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                return def;
            }
            return s;
        }

        public virtual int Get(string key, int def = 0)
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                return def;
            }
            int result;
            return Int32.TryParse(s, out result) ? result : def;
        }

        public virtual long Get(string key, long def = 0)
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                return def;
            }
            long result;
            return Int64.TryParse(s, out result) ? result : def;
        }

        public virtual double Get(string key, double def = 0)
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                return def;
            }
            double result;
            return Double.TryParse(s, out result) ? result : def;
        }

        public virtual string NoDefault(string key)
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                throw new ArgumentException("No configuration value found for parameter: " + key);
            }
            return s;
        }

        public virtual T NoDefault<T>(string key)
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                throw new ArgumentException("No configuration value found for parameter: " + key);
            }
            return (T)Convert.ChangeType(s, typeof(T));
        }

        public virtual T Get<T>(string key, T def = default(T))
        {
            string s = GetSettingFromKey(key);
            if (s == null)
            {
                return def;
            }
            return (T)Convert.ChangeType(s, typeof(T));
        }

        /// <summary>
        /// Return string-valued setting for key, or null if no setting with that key is available
        /// </summary>
        /// <param name="key">String that identifies setting</param>
        /// <returns>Value of setting, or null</returns>
        public abstract string GetSettingFromKey(string key);
    }

    /// <summary>
    /// Implementation of IAppConfig that gets values from app settings in configuration file
    /// </summary>
    [Export(typeof(AppConfig))]
    [Export(typeof(IAppConfig))]
    public class AppConfig : AppConfigBase
    {
        IConfiguration _configuration;
        [ImportingConstructor]
        public AppConfig(IConfiguration configuration)
        {
        }
        /// <summary>
        /// Return string-valued setting for key from app settings in configuration file
        /// </summary>
        /// <param name="key">String that identifies setting</param>
        /// <returns>Value of setting, or null</returns>
        public override string GetSettingFromKey(string key)
        {
            return _configuration[key];
        }

        public IConfiguration Configuration => _configuration;
    }
}
