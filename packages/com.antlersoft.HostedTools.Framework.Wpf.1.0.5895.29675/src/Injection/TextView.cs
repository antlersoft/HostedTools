using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class TextView : SettingViewBase
    {
        private readonly TextBox _element;

        public TextView()
        {
            _element = new TextBox();
            _element.IsReadOnly = false;
            _element.KeyUp += delegate { CheckForChange(); };
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.Text);
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _element.Text = Setting.GetRaw();
            SetNeedsSave(false);
        }

        public override FrameworkElement GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener((s) => _element.Dispatcher.BeginInvoke(new Action(Reset)));
            _element.ToolTip = Setting.Definition.Description;
            Reset();
            return _element;
        }

        private void CheckForChange()
        {
            SetNeedsSave(_element.Text != Setting.GetRaw());
        }
    }
}
