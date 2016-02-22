using System.Windows;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal abstract class SettingViewBase : HostedObjectBase, ISavable, IElementSource
    {
        private bool _needsSave;
        private readonly ListenerCollection<ISavable> _needsSaveChangedListeners = new ListenerCollection<ISavable>();
        
        internal SettingViewBase()
        {
            _needsSaveChangedListeners = new ListenerCollection<ISavable>();
        }

        internal virtual ISetting Setting { get; set; }

        public abstract bool TrySave();

        public bool NeedsSave()
        {
            return _needsSave;
        }

        public abstract void Reset();

        public IListenerCollection<ISavable> NeedsSaveChangedListeners { get { return _needsSaveChangedListeners;  } }

        public abstract FrameworkElement GetElement(object container);

        internal void SetNeedsSave(bool needsSave)
        {
            bool needsNotify = (needsSave != _needsSave);
            _needsSave = needsSave;
            if (needsNotify)
            {
                _needsSaveChangedListeners.NotifyListeners(this);
            }
        }
    }
}
