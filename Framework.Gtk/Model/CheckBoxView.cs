using System;
using System.Collections.Generic;
using System.Text;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class CheckBoxView : SettingViewBase
    {
        private CheckButton _element;

        public CheckBoxView()
        {
            _element = new CheckButton();
            _element.Clicked += delegate { CheckForChange(); };
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.Active  ? "true" : "false");
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _element.Active = Setting.Get<bool>();
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
            SetNeedsSave(_element.Active != Setting.Get<bool>());
        }
    }
}
