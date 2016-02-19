using System;
using System.Windows;
using System.Windows.Controls;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class CheckBoxView : SettingViewBase
    {
        private CheckBox _element;

        public CheckBoxView()
        {
            _element = new CheckBox();
            _element.Checked += delegate { CheckForChange(); };
            _element.Unchecked += delegate { CheckForChange(); };
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.IsChecked ?? false ? "true" : "false");
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _element.IsChecked = Setting.Get<bool>();
            SetNeedsSave(false);
        }

        public override FrameworkElement GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener((s) =>
            { _element.Dispatcher.BeginInvoke(new Action(() => { Reset(); })); });
            _element.ToolTip = Setting.Definition.Description;
            Reset();
            return _element;
        }

        private void CheckForChange()
        {
            SetNeedsSave(_element.IsChecked != Setting.Get<bool>());
        }
    }
}
