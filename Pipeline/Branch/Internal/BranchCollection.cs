using System;
using System.Collections.Generic;
using System.Threading;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch.Internal {
    internal class BranchCollection : HostedObjectBase, IBranchCollection
    {
        List<HtValueBranch> _branches = new List<HtValueBranch>();
        public string Key { get; private set; }

        public int Count => _branches.Count;

        public bool IsFinished {get; private set;}

        private readonly object _lock = new object();
        private INodeStore _nodeStore;
        private IPluginManager _pluginManager;
        private IWorkMonitor _monitor;

        internal BranchCollection(string key, INodeStore nodeStore, IPluginManager pluginManager, IWorkMonitor monitor) {
            Key = key;
            _nodeStore = nodeStore;
            _pluginManager = pluginManager;
            _monitor = monitor;
        }

        public IHtValueSource GetHtValueSource(int index)
        {
            lock (_lock) {
                if (index < 0 || index >= Count) {
                    throw new InvalidOperationException($"No branch with index {index} in space {Key}");
                }
                return _branches[index];
            }
        }

        private bool HasBranchSource(PluginState state, int addedIndex) {
            string branchKey;
            if (state.SettingValues.TryGetValue(BranchSource.SourceBranchKey.FullKey(), out branchKey)) {
                if (branchKey == Key) {
                    if (state.SettingValues.ContainsKey(BranchSource.BranchIndex.FullKey())) {
                        state.SettingValues[BranchSource.BranchIndex.FullKey()] = $"{addedIndex}";
                    }
                    return true;
                }
            }
            foreach (var nested in state.NestedValues) {
                if (HasBranchSource(nested.Value, addedIndex)) {
                    return true;
                }
            }
            return false;
        }

        public IBranchHtValueReceiver GetNextReceiver(Func<IHtValue> producer)
        {
            HtValueBranch newBranch = null;
            int addedIndex=0;
            lock (_lock) {
                if (IsFinished) {
                    throw new InvalidOperationException($"GetNextReceiver calles on Finished Key {Key}");
                }
                addedIndex = Count;
                newBranch = new HtValueBranch(producer, Key, addedIndex);
                _branches.Add(newBranch);
            }

            // Look for a pipeline that needs to be started to read the other end of this branch
            foreach (var pipeline in _nodeStore.GetMatching(typeof(IWorkNode), "")) {
                string destinationValue;
                if (pipeline.State.SettingValues.TryGetValue(PipelinePlugin.Sink.FullKey(), out destinationValue)) {
                    if (destinationValue != typeof(NullSink).FullName) {
                        if (HasBranchSource(pipeline.State, addedIndex)) {
                            var node = _pluginManager[pipeline.State.PluginName]?.Cast<IWorkNode>();
                            if (node == null) {
                                _monitor.Writer.WriteLine($"Couldn't find plugin to execute work to receive branch output on {Key}");
                            } else {
                                ThreadPool.QueueUserWorkItem((s) => s.Perform(pipeline.State, _monitor), node, false);
                            }
                            break;
                        }
                    }
                }
            }

            return newBranch;
        }

        internal void Finish() {
            lock (_lock) {
                if (IsFinished) {
                    return;
                }
                IsFinished = true;
                foreach (var i in _branches) {
                    i.Finish();
                }
            }
        }
    }
}