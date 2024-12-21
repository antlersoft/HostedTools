using System;
using System.Collections.Generic;
using System.Threading;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch.Internal {
    class HtValueBranch : HostedObjectBase, IBranchHtValueReceiver, IHtValueSource
    {
        private Queue<IHtValue> _receivedRows = new Queue<IHtValue>();
        private bool _finished = false;
        private object _lock = new object();
        private SemaphoreSlim _semaphore;
        internal HtValueBranch(string key, int index=0) {
            BranchKey = key;
            Index = index;
            _semaphore= new SemaphoreSlim(1,1);
            // Semaphore is taken whenever the queue is empty
            // and finished is false
            _semaphore.Wait();
        }

        public string BranchKey { get; private set; }

        public int Index { get; private set; }

        public void Finish()
        {
            lock (_lock) {
                _finished=true;
                if (_semaphore.CurrentCount == 0) {
                    _semaphore.Release();
                }
            }
        }

        public IEnumerable<IHtValue> GetRows(IWorkMonitor monitor)
        {
            while (true) {
                _semaphore.Wait();
                IHtValue result = null;
                lock (_lock) {
                    if (_receivedRows.Count > 0) {
                        result = _receivedRows.Dequeue();
                    }
                    if (_receivedRows.Count > 0 || _finished) {
                        _semaphore.Release();
                    }
                }
                if (result != null) {
                    yield return result;
                } else if (_finished) {
                    break;
                }
            }
        }

        public void ReceiveRow(IHtValue row)
        {
            lock (_lock) {
                if (_finished) {
                    throw new InvalidOperationException($"{BranchKey}[{Index}]:ReceiveRow after finish");
                }
                bool wasEmpty = _receivedRows.Count == 0;
                if (wasEmpty && _semaphore.CurrentCount > 0) {
                    throw new InvalidOperationException($"{BranchKey}[{Index}]:ReceiveRow with empty queue but semaphore available");
                }
                _receivedRows.Enqueue(row);
                if (wasEmpty) {
                    _semaphore.Release();
                }
            }
        }
    }
}