using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Pipeline
{
    public class PluginState
    {
        public string PluginName;
        public Dictionary<string, string> SettingValues;
        public Dictionary<string, PluginState> NestedValues;
    }
}
