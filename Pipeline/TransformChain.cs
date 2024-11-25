using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
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
        static readonly string EditButton = "Edit item at";
        static readonly string InsertButton = "Insert before";
        static readonly string DeleteButton = "Delete";
        static readonly string ReplaceButton = "Replace item at";

        private static ISettingDefinition ChainDefinition = new MultiLineSettingDefinition("ChainDefinition", "Pipeline",
            10, "Chain Definition", "Serialized version of transforms in the chain", null, null, false);
        private static ISettingDefinition ChainButtons = new ButtonsDefinition("ChainButtons", "Pipeline", new[] {"Add transform", "Remove last transform"});
        private static ISettingDefinition ChainDescription = new MultiLineSettingDefinition("ChainDescription", "Pipeline", 10, "Description");
        private static ISettingDefinition LinkIndex = new SimpleSettingDefinition("LinkIndex","Pipeline.ChainTransform", "Index", "Index of item in chain (1-based)", typeof(int), "1", false, 0);
        private static ISettingDefinition IndexButtons = new ButtonsDefinition("IndexButtons", "Pipeline.ChainTransform", new [] { EditButton, ReplaceButton, InsertButton, DeleteButton});

        [Import]
        IJsonFactory JsonFactory { get; set; }

        [Import]
        INavigationManager NavigationManager {get;set;}

        public TransformChain()
            : base(new MenuItem("DevTools.Pipeline.Transform.Chain", "Chain transforms", typeof(TransformChain).FullName, "DevTools.Pipeline.Transform"), new[] { ChainDefinition.FullKey(), PipelinePlugin.Transform.FullKey(), ChainButtons.FullKey(), LinkIndex.FullKey(), IndexButtons.FullKey(), ChainDescription.FullKey() })
        {
            ChainDescription.InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(true)); 
            IndexButtons.InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(false));
            ChainButtons.InjectImplementation(typeof(IReadOnly), new CanBeReadOnly(false));           
        }

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
            (ChainButtons.Cast<IReadOnly>() as CanBeReadOnly).KeyReadOnly("Remove last transform", i==0);
            int index=LinkIndex.Value<int>(SettingManager);
            bool enabled = (index > 0 && index <= i);
            CanBeReadOnly indexReadOnly = IndexButtons.Cast<IReadOnly>() as CanBeReadOnly;
            indexReadOnly.KeyReadOnly(EditButton, ! enabled);
            indexReadOnly.KeyReadOnly(InsertButton, ! enabled);
            indexReadOnly.KeyReadOnly(DeleteButton, ! enabled);
            indexReadOnly.KeyReadOnly(ReplaceButton, ! enabled);
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
	        get { return new[] {ChainDefinition, ChainButtons, ChainDescription, LinkIndex, IndexButtons}; }
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
                    },
                    {
                        LinkIndex.FullKey(), (m,s) => {
                            UpdateChainDescription();
                        }
                    },
                    {
                        IndexButtons.FullKey(), (m,s) => {
                            int index=LinkIndex.Value<int>(SettingManager);
                            List<PluginState> states = GetExisting();
                            if (index < 1 || index > states.Count) {
                                m.Writer.WriteLine($"{index} not valid plugin index");
                                return;
                            }
                            string pressed = s.Get<string>();
                            var selected = states[index - 1];
                            if (pressed==EditButton) {
                                PluginManager[selected.PluginName].Cast<IPipelineNode>()?.SetPluginState(selected);
                                NavigationManager.NavigateTo(states[index-1].PluginName);
                                return;
                            } else if (pressed == ReplaceButton) {
                                IPlugin transform =
                                    ((PluginSelectionItem)
                                        PipelinePlugin.Transform.FindMatchingItem(
                                            PipelinePlugin.Transform.Value<string>(SettingManager))).Plugin;
                                IPipelineNode node = transform.Cast<IPipelineNode>();
                                if (node != null)
                                {
                                    states[index-1]=node.GetPluginState();
                                }
                            } else if (pressed == InsertButton) {
                                IPlugin transform =
                                    ((PluginSelectionItem)
                                        PipelinePlugin.Transform.FindMatchingItem(
                                            PipelinePlugin.Transform.Value<string>(SettingManager))).Plugin;
                                IPipelineNode node = transform.Cast<IPipelineNode>();
                                if (node != null)
                                {
                                    states.Insert(index-1, node.GetPluginState());
                                }
                            } else if (pressed==DeleteButton) {
                                states.RemoveAt(index-1);
                            }
                            SettingManager[ChainDefinition.FullKey()].SetRaw(
                                        JsonConvert.SerializeObject(states,
                                            JsonFactory.GetSettings(true)));
                        }
                    }
                };
            }
        }

        public IEnumerable<string> RuntimeSettingKeys => new[] { ChainDefinition.FullKey() };
    }
}
