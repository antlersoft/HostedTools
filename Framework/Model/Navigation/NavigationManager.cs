using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;

namespace com.antlersoft.HostedTools.Framework.Model.Navigation
{
    [Export(typeof(INavigationManager))]
    public class NavigationManager : INavigationManager
    {
        private readonly object _lock = new object();
        private readonly List<string> _history = new List<string>();
        private readonly List<string> _forward = new List<string>();
        private readonly ListenerCollection<INavigationManager> _listeners = new ListenerCollection<INavigationManager>(); 
        private string _current;
 
        public string CurrentLocation
        {
            get
            {
                lock (_lock)
                {
                    var result = _current;
                    return result;
                }
            }
        }

        public void NavigateTo(string destination)
        {
            lock (_lock)
            {
                if (_current == destination)
                {
                    return;
                }
                string oldCurrent = _current;
                List<string> oldForward = _forward.ToList();
                List<string> oldHistory = _history.ToList();
                if (_current != null)
                {
                    if (_history.Count > 0 && _history[0] == destination)
                    {
                        _forward.Insert(0, _current);
                        _history.RemoveAt(0);
                    }
                    else
                    {
                        _history.Insert(0, _current);
                        if (_forward.Count > 0 && destination == _forward[0])
                        {
                            _forward.RemoveAt(0);
                        }
                        else
                        {
                            _forward.Clear();
                        }
                    }
                }
                _current = destination;
                try
                {
                    _listeners.NotifyListeners(this);
                }
                catch (Exception ex)
                {
                    _current = oldCurrent;
                    _forward.Clear();
                    _forward.AddRange(oldForward);
                    _history.Clear();
                    _history.AddRange(oldHistory);
                    if (! (ex is RejectNavigationException))
                    {
                        throw;
                    }
                }
            }
        }

        public void GoBack()
        {
            lock (_lock)
            {
                if (_history.Count > 0)
                {
                    NavigateTo(_history[0]);
                }
            }
        }

        public void GoForward()
        {
            lock (_lock)
            {
                if (_forward.Count > 0)
                {
                    NavigateTo(_forward[0]);
                }
            }
        }

        public IList<string> History
        {
            get
            {
                lock (_lock)
                {
                    return _history.ToList();
                }
            }
        }

        public IList<string> Forward
        {
            get
            {
                lock (_lock)
                {
                    return _forward.ToList();
                }
            }
        }

        public IListenerCollection<INavigationManager> NavigationListeners
        {
            get { return _listeners; }
        }
    }
}
