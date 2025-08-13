using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Pipeline.Branch.Internal;
using com.antlersoft.HostedTools.Pipeline.Extensions;

namespace com.antlersoft.HostedTools.Pipeline.Branch {

    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class Tee : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource {
        [Import]
        public IBranchManager BranchManager { get; set; }

        public override string NodeDescription => $"Write a copy to branch {BranchKey.Value<string>(SettingManager)}";

        public IEnumerable<ISettingDefinition> Definitions => new [] { BranchKey };

        public static ISettingDefinition BranchKey = new SimpleSettingDefinition("BranchKey", "Pipeline", "Branch Key", "Identifies the branch to match with other end");

        public Tee()
        : base(new MenuItem("DevTools.Pipeline.Transform.Tee", "Tee branch", typeof(Tee).FullName, "DevTools.Pipeline.Transform"),
            new[] { BranchKey.FullKey() })
        {

        }

        class Transform : HostedObjectBase, IHtValueTransform, IDisposable
        {
            private readonly IBranchManager _branchManager;
            private readonly string _key;

            private IWorkMonitor _monitor;
            private IBranchCollection _collection;
            private IBranchHtValueReceiver _receiver;
            internal Transform(IBranchManager branchManager, string key) {
                _branchManager = branchManager;
                _key = key;
            }

            public void Dispose() {
                if (_receiver != null) {
                    _receiver.Finish();
                    _receiver = null;
                }
                if (_monitor!= null) {
                    _branchManager.FinishBranchCollection(_monitor, _key);
                    _monitor = null;
                    _collection = null;
                }
            }

            private IEnumerable<IHtValue> InternalGetTransformed(IEnumerable<IHtValue> input)
            {
                foreach (var row in input)
                {
                    _receiver.ReceiveRow(row);
                    yield return row;
                }
                _receiver.Finish();
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                _monitor = monitor;
                _collection = _branchManager.CreateBranchCollection(_monitor, _key);
                _receiver = _collection.GetNextReceiver();

                return InternalGetTransformed(input);
            }
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(BranchManager, state.SettingValues[BranchKey.FullKey()]);
        }
    }
}