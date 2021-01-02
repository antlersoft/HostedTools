using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Utility.Testing
{
    public class TestingConfig : AppConfig
    {
        public TestingConfig(IConfiguration configuration)
        : base(configuration)
        {
            ConfigOverrides = new Dictionary<string, string>();
        }
        public TestingConfig(IConfiguration configuration, Dictionary<string,string> configOverrides)
        : base(configuration)
        {
            ConfigOverrides = configOverrides;
        }
        public Dictionary<string, string> ConfigOverrides { get; set; } 
        public override string GetSettingFromKey(string key)
        {
            if (ConfigOverrides != null)
            {
                string val;
                ConfigOverrides.TryGetValue(key, out val);
                return val;
            }
            return base.GetSettingFromKey(key);
        }
    }
}
