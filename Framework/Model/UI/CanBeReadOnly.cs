using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Model.UI {
    public class CanBeReadOnly : IReadOnly, IAggregatable
    {
        private INotifiableListenerCollection<IReadOnly> _listeners = new ListenerCollection<IReadOnly>();
        private IHostedObject _aggregator;
        private Dictionary<string,bool> _keyReadOnly = new Dictionary<string, bool>();
        private object _lock = new object();
        private bool _readOnly = false;
        public IListenerCollection<IReadOnly> ReadOnlyChangeListeners => _listeners;

        public CanBeReadOnly(bool initialValue = false) {
            _readOnly = initialValue;
        }

        public bool ReadOnly { 
            get { return _readOnly; }
            set {
                bool needNotify;
                lock (_lock) {
                    needNotify = value != _readOnly;
                    _readOnly = value;
                }
                if (needNotify) {
                    _listeners.NotifyListeners(this);
                }
            }
        }
        public bool KeyReadOnly(string key) {
            bool result;
            lock (_lock) {
                if (! _keyReadOnly.TryGetValue(key, out result)) {
                    return false;
                }
            }
            return result;
        }

        public void KeyReadOnly(string key, bool val) {
            bool needNotify = false;
            lock (_lock) {
                if (KeyReadOnly(key)!=val) {
                    _keyReadOnly[key] = val;
                    needNotify = true;
                }
            }
            if (needNotify) {
                _listeners.NotifyListeners(this);
            }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            IHostedObject aggregator = _aggregator;
            if (! fromAggregated && aggregator != null) {
                return aggregator.Cast<T>();
            }
            return this as T;
        }

        public bool IsReadOnly(string key = null)
        {
            if (key == null) {
                return ReadOnly;
            }
            return KeyReadOnly(key);
        }

        public void SetAggregator(IHostedObject aggregator)
        {
            _aggregator = aggregator;
        }
    }
}