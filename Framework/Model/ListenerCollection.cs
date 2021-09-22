using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Framework.Model
{
    public class ListenerCollection<T> : INotifiableListenerCollection<T>
    {
        private readonly HashSet<Action<T>> _listeners = new HashSet<Action<T>>();
 
        public void NotifyListeners(T item)
        {
            lock (_listeners)
            {
                foreach (var l in _listeners)
                {
                    l.Invoke(item);
                }
            }
        }

        public void AddListener(Action<T> listener)
        {
            lock (_listeners)
            {
                _listeners.Add(listener);
            }
        }

        public void RemoveListener(Action<T> listener)
        {
            lock (_listeners)
            {
                _listeners.Remove(listener);
            }
        }
    }
}
