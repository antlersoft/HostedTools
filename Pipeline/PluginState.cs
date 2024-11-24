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
        public string Key;

        public bool IsSameAs(object obj)
        {
            if (obj is PluginState other) {
                return PluginName == other.PluginName && Key == other.Key &&
                    ((SettingValues == null && other.SettingValues == null) || (
                    SettingValues != null && other.SettingValues != null &&
                    SettingValues.Count == other.SettingValues.Count &&
                    SettingValues.All(kvp => other.SettingValues.ContainsKey(kvp.Key) && other.SettingValues[kvp.Key]==kvp.Value))) &&
                    ((NestedValues == null && other.NestedValues == null) || (
                    NestedValues != null && other.NestedValues != null &&
                    NestedValues.Count == other.NestedValues.Count &&
                    NestedValues.All(kvp => other.NestedValues.ContainsKey(kvp.Key) && other.NestedValues[kvp.Key].IsSameAs(kvp.Value))));
            }
            return false;
        }
    }
}
