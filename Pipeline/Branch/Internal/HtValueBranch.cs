using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch.Internal {
    class HtValueBranch : HostedObjectBase, IBranchHtValueReceiver, IHtValueSource
    {
        private bool _finished = false;
        private object _lock = new object();
        private Func<IHtValue> _producer;
        internal HtValueBranch(Func<IHtValue> producer, string key, int index = 0)
        {
            BranchKey = key;
            Index = index;
            _producer = producer;
        }

        public string BranchKey { get; private set; }

        public int Index { get; private set; }

        public void Finish()
        {
            lock (_lock) {
                _finished=true;
            }
        }

        public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
        {
            for (IHtValue row = _producer(); row != null; row = _producer())
            {
                yield return row;
            }
        }

    }
}