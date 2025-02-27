﻿using System;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    class ComboBoxTextWithEntry : ComboBoxText
    {
        internal ComboBoxTextWithEntry(bool withEntry)
        : base(withEntry)
        { }
    }
    internal class ComboBoxView : SettingViewBase
    {
        protected readonly ComboBoxText _element;

        public ComboBoxView()
        : this(true)
        {

        }

        internal override ISetting Setting {
            set {
                base.Setting = value;
                if (Setting.Definition.Cast<IReadOnly>() is IReadOnly ro) {
                    _element.Sensitive = ! ro.IsReadOnly();
                    if (_element.HasEntry)
                        _element.Child.Sensitive = ! ro.IsReadOnly();
                    ro.ReadOnlyChangeListeners.AddListener(
                        (a) => { Application.Invoke(delegate {
                            _element.Sensitive = ! a.IsReadOnly();
                            if (_element.HasEntry)
                                _element.Child.Sensitive = ! ro.IsReadOnly();
                        }); }
                    );
                }
            }
        }

        protected ComboBoxView(bool withEntry)
        {
            _element = new ComboBoxTextWithEntry(withEntry);
            //_element.IsEditable = true;
            //_element.IsReadOnly = false;
            _element.EditingDone += delegate { CheckForChange(); };
            _element.Changed += delegate (object sender, EventArgs args)
            {
                if (_element.ActiveText != null)
                {
                    SetNeedsSave(_element.ActiveText != Setting.GetRaw());
                }
            };
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.ActiveText);
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _element.Model = new ListStore(typeof(string));
            foreach (var val in Setting.PreviousValues)
            {
                _element.AppendText(val);
            }
            if (Setting.PreviousValues.Count == 0 && Setting.GetRaw() != null)
            {
                _element.AppendText(Setting.GetRaw());
            }
            _element.Active = 0;
            SetNeedsSave(false);
        }

        public override Widget GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener((s) =>
            { Application.Invoke(delegate { Reset(); }); });
            _element.TooltipText = Setting.Definition.Description;
            Reset();
            return _element;
        }

        private void CheckForChange()
        {
            SetNeedsSave(_element.ActiveText != Setting.GetRaw());
        }
    }
}
