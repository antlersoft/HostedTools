using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Pipeline.Extensions {

public static class NodeExtension {
    public static PluginState AssemblePluginState(this IPipelineNode node, IPluginManager pluginManager, ISettingManager settings) {
        PluginState result = new PluginState();
        if (node.Cast<IPlugin>() is IPlugin plugin) {
            result.PluginName = plugin.Name;
        } else {
            result.PluginName = node.GetType().FullName;
        }
        result.NestedValues = new Dictionary<string,PluginState>();
        result.SettingValues = new Dictionary<string, string>();
        IEnumerable<string> settingKeys = null;
        if (node.Cast<IRuntimeStateSettings>() is IRuntimeStateSettings stateSettings) {
            settingKeys = stateSettings.RuntimeSettingKeys;
        } else {
            if (node.Cast<ISettingEditList>() is ISettingEditList settingEditList) {
                settingKeys = settingEditList.KeysToEdit;
            }
        }

        if (settingKeys != null) {
            foreach (var setting in settingKeys) {
                ISetting settingObject = settings[setting];
                var settingRaw = settingObject.GetRaw();
                result.SettingValues[setting]=settingRaw;
                if (settingObject.Definition.Cast<PluginSelectionSettingDefinition>() is PluginSelectionSettingDefinition psd) {
                    var nestedPlugin = pluginManager[settingRaw];
                    if (nestedPlugin.Cast<IPipelineNode>() is IPipelineNode nestedNode) {
                        result.NestedValues[setting] = nestedNode.AssemblePluginState(pluginManager, settings);
                    }
                }
            }
        }

        return result;
    }
}
}