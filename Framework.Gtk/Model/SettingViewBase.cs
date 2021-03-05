using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
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

        public IListenerCollection<ISavable> NeedsSaveChangedListeners { get { return _needsSaveChangedListeners; } }

        public abstract Widget GetElement(object container);

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
