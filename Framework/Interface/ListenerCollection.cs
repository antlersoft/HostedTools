using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface
{
    public interface IListenerCollection<out T>
    {
        void AddListener(Action<T> listener);
        void RemoveListener(Action<T> listener);
    }

    public interface INotifiableListenerCollection<T> : IListenerCollection<T>
    {
        void NotifyListeners(T item);
    }
}
