using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.Menu
{
    public interface IMenuManager
    {
        IEnumerable<IMenuItem> GetChildren(IMenuItem parent);
        IMenuItem this[string key] { get; }
        void Add(IMenuItem item);
        void Remove(string key);
        void AddChangeListener(Action listener);
        void RemoveChangeListener(Action listener);
    }
}
