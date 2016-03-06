using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Framework.Model
{
    public class HostedObjectBase : IAggregatable, IAggregator
    {
        private List<KeyValuePair<Type, IHostedObject>> _injections;
        private IHostedObject _aggregator;
        private object _lock = new object();
        private IHostedObject _myself;

        public HostedObjectBase()
        {
            _myself = this;
        }

        public HostedObjectBase(IHostedObject myself)
        {
            _myself = myself;
        }
 
        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            lock (_lock)
            {
                if (! fromAggregated && _aggregator != null)
                {
                    return _aggregator.Cast<T>();
                }
                if (_injections != null)
                {
                    T result = _myself as T;
                    if (result != null)
                    {
                        return result;
                    }
                    Type t = typeof(T);
                    foreach (var kvp in _injections)
                    {
                        if (t.IsAssignableFrom(kvp.Key))
                        {
                            result = kvp.Value.Cast<T>(true);
                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }
                }
            }
            return this as T;
        }

        public void SetAggregator(IHostedObject aggregator)
        {
            _aggregator = aggregator;
        }

        public bool InjectImplementation(Type t, IAggregatable impl)
        {
            if (t.IsAssignableFrom(GetType()))
            {
                return false;
            }
            lock (_lock)
            {
                impl.SetAggregator(_myself);
                if (_injections == null)
                {
                    _injections = new List<KeyValuePair<Type, IHostedObject>>();
                }
                _injections.Add(new KeyValuePair<Type, IHostedObject>(t, impl));
                return true;
            }
        }
    }
}
