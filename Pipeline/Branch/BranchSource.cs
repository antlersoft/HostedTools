using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch {
    [Export(typeof(IRootNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class BranchSource : AbstractPipelineNode, IHtValueRoot, ISettingDefinitionSource
    {
        [Import]
        public IBranchManager BranchManager { get; set; }

        public override string NodeDescription => $"Read from branch {SourceBranchKey.Value<string>(SettingManager)}";

        public IEnumerable<ISettingDefinition> Definitions => new [] { SourceBranchKey, BranchIndex };

        public static ISettingDefinition SourceBranchKey = new SimpleSettingDefinition("SourceBranchKey", "Pipeline", "Branch Key", "Identifies the branch to match with other end");
        public static ISettingDefinition BranchIndex = new SimpleSettingDefinition("SourceBranchIndex", "Pipeline", "Branch index", "Sequence number when there are multiple branches spawned with same key; default 0", typeof(int), "0");

        public BranchSource()
        : base(new MenuItem("DevTools.Pipeline.Input.Branch", "From a branch", typeof(BranchSource).FullName, "DevTools.Pipeline.Input"),
            new[] { SourceBranchKey.FullKey(), BranchIndex.FullKey() })
        {

        }

        class Source : IHtValueSource {
            private readonly IBranchManager _branchManager;
            private readonly string _key;

            private readonly int _index;

            internal Source(IBranchManager manager, string key, int index) {
                _branchManager = manager;
                _key = key;
                _index = index;
            }

            public T Cast<T>(bool fromAggregated = false) where T : class
            {
                return this as T;
            }

            public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
            {
                return _branchManager.RetrieveBranchCollection(monitor, _key).GetHtValueSource(_index).GetRows(monitor);
            }
        }

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return new Source(BranchManager, state.SettingValues[SourceBranchKey.FullKey()], (int)Convert.ChangeType(state.SettingValues[BranchIndex.FullKey()], typeof(int)));
        }
    }
}