using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Pipeline.Branch.Internal {
    [Export(typeof(IBranchManager))]
    public class BranchManager : IBranchManager
    {
        [Import]
        public INodeStore NodeStore { get; set; }
        [Import]
        public IPluginManager PluginManager { get; set; }

        private Dictionary<IWorkMonitor,Dictionary<string,BranchCollection>> _collections=new Dictionary<IWorkMonitor, Dictionary<string, BranchCollection>>();
        private readonly object _lock = new object();
        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public IBranchCollection CreateBranchCollection(IWorkMonitor monitor, string key)
        {
            lock (_lock) {
                Dictionary<string,BranchCollection> collections;
                if (! _collections.TryGetValue(monitor, out collections)) {
                    collections = new Dictionary<string, BranchCollection>();
                    _collections.Add(monitor, collections);
                }
                BranchCollection collection;
                if (! collections.TryGetValue(key, out collection)) {
                    collection = new BranchCollection(key, NodeStore, PluginManager, monitor);
                    collections.Add(key, collection);
                }
                return collection;
            }
        }

        public void FinishBranchCollection(IWorkMonitor monitor, string key)
        {
            lock (_lock) {
                Dictionary<string,BranchCollection> collections;
                if (_collections.TryGetValue(monitor, out collections)) {
                    BranchCollection collection;
                    if (collections.TryGetValue(key, out collection)) {
                        collection.Finish();
                        collections.Remove(key);
                    }
                    if (collection.Count == 0) {
                        _collections.Remove(monitor);
                    }
                }
            }
        }

        public IBranchCollection RetrieveBranchCollection(IWorkMonitor monitor, string key)
        {
            lock (_lock) {
                Dictionary<string,BranchCollection> collections;
                if (_collections.TryGetValue(monitor, out collections)) {
                    BranchCollection collection;
                    if (collections.TryGetValue(key, out collection)) {
                        return collection;
                    }
                }
            }
            throw new InvalidOperationException($"No collection {key} exists in monitor");
        }
    }
}