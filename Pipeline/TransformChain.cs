using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Plugin.Internal;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline.Extensions;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IAfterComposition))]
    public class TransformChain : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource, IHasSettingChangeActions, IRuntimeStateSettings,
        IAfterComposition
    {
        private static ISettingDefinition ChainDefinition = new MultiLineSettingDefinition("ChainDefinition", "Pipeline",
            10, "Chain Definition", "Serialized version of transforms in the chain", null, null, false);
        private static ISettingDefinition ChainButtons = new ButtonsDefinition("ChainButtons", "Pipeline", new[] {"Add transform", "Remove last transform"});
        private static ISettingDefinition ChainDescription = new MultiLineSettingDefinition("ChainDescription", "Pipeline", 10, "Description");

        [Import]
        IJsonFactory JsonFactory { get; set; }

        public TransformChain()
            : base(new MenuItem("DevTools.Pipeline.Transform.Chain", "Chain transforms", typeof(TransformChain).FullName, "DevTools.Pipeline.Transform"), new[] { ChainDefinition.FullKey(), PipelinePlugin.Transform.FullKey(), ChainButtons.FullKey(), ChainDescription.FullKey() })
        {
            ChainDescription.InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(true));            
        }

        static readonly Type[] EmptyTypeList = new Type[0];
        static readonly object[] EmptyParamList = new object[0];

        public override string NodeDescription
        {
            get { return GetExisting().Count + " transforms"; }
        }

        private List<PluginState> GetExisting(string existingStr)
        {
            List<PluginState> existing;
            if (String.IsNullOrEmpty(existingStr))
            {
                existing = new List<PluginState>();
            }
            else
            {
                try
                {
                    existing = JsonConvert.DeserializeObject<List<PluginState>>(existingStr);
                }
                catch (JsonException)
                {
                    existing = new List<PluginState>();
                }
            }
            return existing;
        }

        private List<PluginState> GetExisting()
        {
            return GetExisting(ChainDefinition.Value<string>(SettingManager));
        }

        class Transform : HostedObjectBase, IHtValueTransform, IDisposable {
            private readonly List<PluginState> _pluginStates;
            private List<IDisposable> _disposables = new List<IDisposable>();
            private readonly IPluginManager _pluginManager;
            internal Transform(List<PluginState> pluginStates, IPluginManager pluginManager) {
                _pluginStates = pluginStates;
                _pluginManager = pluginManager;
            }
            public void Dispose()
            {
                foreach (var i in _disposables) {
                    i.Dispose();
                }
                _disposables.Clear();
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                foreach (PluginState pluginState in _pluginStates)
                {
                    IPlugin nextPlugin = _pluginManager[pluginState.PluginName];
                    var transform = nextPlugin.Cast<IHtValueStem>().GetHtValueTransform(pluginState);
                    var disposable = transform.Cast<IDisposable>();
                    if (disposable != null) {
                        _disposables.Insert(0, disposable);
                    }
                    input = transform.GetTransformed(input, monitor);
                }
                return input;
            }
        }

        private void UpdateChainDescription()
        {
            int i = 0;
            StringBuilder sb=new StringBuilder();
            foreach (var state in GetExisting()) {
                var node = PluginManager[state.PluginName]?.Cast<IPipelineNode>();
                if (node != null) {
                    sb.Append($"{++i}. {node.GetDescriptionFromState(state)}\n");
                }
            }
            if (sb.Length == 0) {
                sb.Append("Empty transform chain");
            }

            SettingManager[ChainDescription.FullKey()].SetRaw(sb.ToString());
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(GetExisting(state.SettingValues[ChainDefinition.FullKey()]), PluginManager);
        }

        public void AfterComposition()
        {
            UpdateChainDescription();
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
	        get { return new[] {ChainDefinition, ChainButtons, ChainDescription}; }
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
                                IPipelineNode node = transform.Cast<IPipelineNode>();
                                if (node != null)
                                {
                                    existing.Add(node.GetPluginState());
                                }
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
                    },
                    {
                        ChainDefinition.FullKey(), (m,s) => {
                            UpdateChainDescription();
                        }
                    }
                };
            }
        }

        public IEnumerable<string> RuntimeSettingKeys => new[] { ChainDefinition.FullKey() };
    }
}
