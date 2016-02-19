using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;

namespace com.antlersoft.HostedTools.Framework.Model.Menu.Internal
{
    [Export(typeof(IMenuManager))]
    public class MenuManager : IMenuManager, IPartImportsSatisfiedNotification
    {
        class MenuNode
        {
            internal IMenuItem Item;
            internal readonly List<MenuNode> Children;

            internal MenuNode(IMenuItem item)
            {
                Children = new List<MenuNode>();
                Item = item;
            }
        }

        private readonly Dictionary<string, MenuNode> _nodes;
        private readonly HashSet<Action> _listeners = new HashSet<Action>(); 
        private object _lock = new object();

        [ImportMany]
        public IEnumerable<IMenuItemSource> Sources;

        public MenuManager()
        {
            _nodes = new Dictionary<string, MenuNode>();
            // Set up empty node
            _nodes[string.Empty] = new MenuNode(null);
        }
 
        public IEnumerable<IMenuItem> GetChildren(IMenuItem parent)
        {
            string parentKey = (parent != null && ! string.IsNullOrEmpty(parent.Key)) ? parent.Key : string.Empty;
            lock (_lock)
            {
                return _nodes[parentKey].Children.Select(c => c.Item).ToList();
            }
        }

        public IMenuItem this[string key]
        {
            get
            {
                lock (_lock)
                {
                    return _nodes[key].Item;
                }
            }
        }

        public void AddChangeListener(Action listener)
        {
            lock (_listeners)
            {
                _listeners.Add(listener);
            }
        }

        public void RemoveChangeListener(Action listener)
        {
            lock (_listeners)
            {
                _listeners.Remove(listener);
            }
        }

        public void Add(IMenuItem item)
        {
            if (String.IsNullOrEmpty(item.Key))
            {
                throw new ArgumentNullException("item", "IMenuItem Key can not be empty");
            }
            lock (_lock)
            {
                MenuNode node;
                string parentKey = item.ParentKey ?? string.Empty;
                if (_nodes.TryGetValue(item.Key, out node))
                {
                    if (node.Item != null)
                    {
                        string oldParentKey = node.Item.ParentKey ?? string.Empty;
                        _nodes[oldParentKey].Children.Remove(node);
                    }
                    node.Item = item;
                }
                else
                {
                    node = new MenuNode(item);
                    _nodes[item.Key] = node;
                }
                MenuNode parentNode;
                if (! _nodes.TryGetValue(parentKey, out parentNode))
                {
                    parentNode = new MenuNode(null);
                    _nodes[parentKey] = parentNode;
                }
                parentNode.Children.Add(node);
                foreach (var listener in _listeners)
                {
                    listener.Invoke();
                }
            }
        }

        public void Remove(string key)
        {
            lock (_lock)
            {
                MenuNode node;
                if (_nodes.TryGetValue(key, out node))
                {
                    if (node.Item != null)
                    {
                        string parentKey = node.Item.ParentKey ?? string.Empty;
                        _nodes[parentKey].Children.Remove(node);
                    }
                    if (node.Children.Count == 0)
                    {
                        _nodes.Remove(key);
                    }
                    else
                    {
                        node.Item = null;
                    }
                    foreach (var listener in _listeners)
                    {
                        listener.Invoke();
                    }
                }
            }
        }

        public void OnImportsSatisfied()
        {
            foreach (var source in Sources)
            {
                foreach (var item in source.Items)
                {
                    Add(item);
                }
            }
        }
    }
}
