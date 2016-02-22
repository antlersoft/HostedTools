using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IHtValueTransform))]
    [Export(typeof(ISettingDefinitionSource))]
    public class TransformChain : EditOnlyPlugin, IHtValueTransform, ISettingDefinitionSource, IHasSettingChangeActions
    {
        private static ISettingDefinition ChainDefinition = new MultiLineSettingDefinition("ChainDefinition", "Pipeline",
            8, "Chain Definition", "Serialized version of transforms in the chain", null, null, false);
        private static ISettingDefinition ChainButtons = new ButtonsDefinition("ChainButtons", "Pipeline", new[] {"Add transform", "Remove last transform"});
        [Import]
        public IPluginManager PluginManager
        {
            get;
            set;
        }

        [Import]
        IJsonFactory JsonFactory { get; set; }

        public TransformChain()
            : base(new MenuItem("DevTools.Pipeline.Transform.Chain", "Chain transforms", typeof(TransformChain).FullName, "DevTools.Pipeline.Transform"), new[] { ChainDefinition.FullKey(), PipelinePlugin.Transform.FullKey(), ChainButtons.FullKey() })
        {
            
        }

        static readonly Type[] EmptyTypeList = new Type[0];
        static readonly object[] EmptyParamList = new object[0];

        public IPlugin GetPluginWithSettingOverrides (string pluginName, Dictionary<string, string> overrides)
		{
			IPlugin compositorPlugin = PluginManager [pluginName];
			Type pluginType = compositorPlugin.GetType ();
			var constructor = compositorPlugin.GetType ().GetConstructor (EmptyTypeList);
			IPlugin overridePlugin = constructor.Invoke (EmptyParamList) as IPlugin;
			foreach (var property in pluginType.GetProperties()) {
				foreach (var attribute in property.GetCustomAttributes(false)) {
					if (attribute.GetType () == typeof(ImportAttribute)) {
						object propertyValue = property.GetValue (compositorPlugin, null);
						if (property.GetGetMethod ().ReturnType == typeof(ISettingManager)) {
							propertyValue = new OverrideValueSettingManager ((ISettingManager)propertyValue, overrides);
						}
						property.SetValue (overridePlugin, propertyValue, null);
					}
				}
			}
			foreach (var fieldInfo in pluginType.GetFields()) {
				foreach (var attribute in fieldInfo.GetCustomAttributes(false))
                {
                    if (attribute.GetType () == typeof(ImportAttribute))
                    {
                        object propertyValue = fieldInfo.GetValue(compositorPlugin);
                        if (fieldInfo.FieldType == typeof(ISettingManager))
                        {
                            propertyValue = new OverrideValueSettingManager((ISettingManager)propertyValue, overrides);
                        }
                        fieldInfo.SetValue(overridePlugin, propertyValue);
                    }
                }
            }
            var afterComposition = overridePlugin.Cast<IAfterComposition>();
            if (afterComposition != null)
            {
                afterComposition.AfterComposition();
            }
            return overridePlugin;
        }

        public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
        {
            foreach (PluginState pluginState in GetExisting())
            {
                IPlugin nextPlugin = GetPluginWithSettingOverrides(pluginState.PluginName, pluginState.SettingValues);
                input = nextPlugin.Cast<IHtValueTransform>().GetTransformed(input, monitor);
            }
            return input;
        }

        public string TransformDescription
        {
            get { return GetExisting().Count + " transforms"; }
        }

        private List<PluginState> GetExisting()
        {
            string existingStr = ChainDefinition.Value<string>(SettingManager);
            List<PluginState> existing;
            if (String.IsNullOrEmpty(existingStr))
            {
                existing = new List<PluginState>();
            }
            else
            {
                existing = JsonConvert.DeserializeObject<List<PluginState>>(existingStr);
            }
            return existing;
        }
    
        public IEnumerable<ISettingDefinition> Definitions
        {
	        get { return new[] {ChainDefinition, ChainButtons}; }
        }

        public Dictionary<string, Action<IWorkMonitor, ISetting>> ActionsBySettingKey
        {
            get
            {
                return new Dictionary<string, Action<IWorkMonitor, ISetting>>
                {
                    {
                        ChainButtons.FullKey(), (m, s) =>
                        {
                            var existing = GetExisting();
                            if (s.Get<string>() == "Add transform")
                            {
                                IPlugin transform =
                                    ((PluginSelectionItem)
                                        PipelinePlugin.Transform.FindMatchingItem(
                                            PipelinePlugin.Transform.Value<string>(SettingManager))).Plugin;
                                Dictionary<string, string> values = new Dictionary<string, string>();
                                ISettingEditList list = transform.Cast<ISettingEditList>();
                                if (list != null)
                                {
                                    foreach (string settingKey in list.KeysToEdit)
                                    {
                                        ISetting setting = SettingManager[settingKey];
                                        if (!(setting.Definition is ButtonsDefinition))
                                        {
                                            values[settingKey] = SettingManager[settingKey].GetRaw();
                                        }
                                    }
                                }
                                existing.Add(new PluginState {PluginName = transform.Name, SettingValues = values});
                                SettingManager[ChainDefinition.FullKey()].SetRaw(JsonConvert.SerializeObject(existing,
                                    JsonFactory.GetSettings(true)));
                            }
                            else
                            {
                                if (existing.Count > 0)
                                {
                                    existing.RemoveAt(existing.Count - 1);
                                    SettingManager[ChainDefinition.FullKey()].SetRaw(
                                        JsonConvert.SerializeObject(existing,
                                            JsonFactory.GetSettings(true)));
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
