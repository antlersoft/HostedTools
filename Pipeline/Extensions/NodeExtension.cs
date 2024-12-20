using System.Collections.Generic;
using System.IO;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Serialization;
using CsvHelper;

namespace com.antlersoft.HostedTools.Pipeline.Extensions {

public static class NodeExtension {
    public static PluginState AssemblePluginState(this IPipelineNode node, IPluginManager pluginManager, ISettingManager settings, ISet<string> visitedPlugins=null) {
        PluginState result = new PluginState();
        if (node.Cast<IPlugin>() is IPlugin plugin) {
            result.PluginName = plugin.Name;
        } else {
            result.PluginName = node.GetType().FullName;
        }
        if (visitedPlugins == null) {
            visitedPlugins = new HashSet<string>();
        }
        if (node.Cast<IHasSaveKey>() is IHasSaveKey saveKey) {
            result.Key = saveKey.SaveKey;
        }
        visitedPlugins.Add(result.PluginName);
        result.NestedValues = new Dictionary<string,PluginState>();
        result.SettingValues = new Dictionary<string, string>();
        IEnumerable<string> settingKeys = null;
        if (node.Cast<IRuntimeStateSettings>() is IRuntimeStateSettings stateSettings) {
            settingKeys = stateSettings.RuntimeSettingKeys;
        } else {
            if (node?.Cast<ISettingEditList>() is ISettingEditList settingEditList) {
                settingKeys = settingEditList.KeysToEdit;
            }
        }

        if (settingKeys != null) {
            foreach (var setting in settingKeys) {
                ISetting settingObject = settings[setting];
                var settingRaw = settingObject.GetRaw();
                result.SettingValues[setting]=settingRaw;
                if (! visitedPlugins.Contains(settingRaw) && settingObject.Definition.Cast<PluginSelectionSettingDefinition>() is PluginSelectionSettingDefinition psd) {
                    var nestedPlugin = pluginManager[settingRaw];
                    if (nestedPlugin?.Cast<IPipelineNode>() is IPipelineNode nestedNode) {
                        result.NestedValues[setting] = nestedNode.GetPluginState(visitedPlugins);
                    }
                }
            }
        }

        return result;
    }

    public static void DeployPluginState(this IPipelineNode node, PluginState state, IPluginManager pluginManager,
        ISettingManager settings, ISet<string> visitedNodes = null) {

        if (node.Cast<IHasSaveKey>() is IHasSaveKey saveKey) {
            saveKey.SaveKey = state.Key;
        }
        foreach (var setting in state.SettingValues) {
            settings[setting.Key].SetRaw(setting.Value);
        }
        string thisName;
        if (node.Cast<IPlugin>() is IPlugin plugin) {
            thisName = plugin.Name;
        } else {
            thisName = node.GetType().FullName;
        }
        if (visitedNodes == null) {
            visitedNodes = new HashSet<string>();
        }
        visitedNodes.Add(thisName);
        if (state.NestedValues!= null) {
            foreach (var nested in state.NestedValues.Values) {
                if (! visitedNodes.Contains(nested.PluginName) &&
                    pluginManager[nested.PluginName].Cast<IPipelineNode>() is IPipelineNode nestedNode) {
                    nestedNode.SetPluginState(nested, visitedNodes);
                }
            }
        }
    }

    static readonly int capacity = 100;
    private static Dictionary<string,string> _stateToDescription=new Dictionary<string,string>();
    private static List<string> _stateKeys = new List<string>(capacity);

    public static string GetDescriptionFromState(this IPipelineNode node, PluginState state) {
        string serialized = null; 
        string result = "null";
        if (state != null) {
            using (var sr = new StringWriter()) {
                new JsonFactory().GetSerializer().Serialize(sr, state);
                serialized = sr.ToString();
            }
            lock (_stateKeys) {
                if (_stateToDescription.TryGetValue(serialized, out result)) {
                    return result;
                }
                _stateToDescription[serialized]=result;
                if (_stateKeys.Count >= capacity) {
                    var toRemove = _stateKeys[0];
                    _stateKeys.RemoveAt(0);
                    _stateToDescription.Remove(toRemove);
                }
                _stateKeys.Add(serialized);
            }
            PluginState existingState = node.GetPluginState();
            node.SetPluginState(state);
            result = node.NodeDescription;
            node.SetPluginState(existingState);
            lock (_stateKeys) {
                if (_stateToDescription.ContainsKey(serialized)) {
                    _stateToDescription[serialized]=result;
                }
            }
        }

        return result;
    }
}
}
