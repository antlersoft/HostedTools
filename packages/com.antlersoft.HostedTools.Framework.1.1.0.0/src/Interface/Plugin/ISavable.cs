using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface ISavable : IHostedObject
    {
        bool TrySave();
        bool NeedsSave();
        void Reset();
        IListenerCollection<ISavable> NeedsSaveChangedListeners { get; }
    }
}
