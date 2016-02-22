using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class PasswordView : SettingViewBase
    {
        private readonly PasswordBox _element;

        public PasswordView()
        {
            _element = new PasswordBox();
            SetNeedsSave(true);
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.Password);
            return true;
        }

        public override void Reset()
        {
            _element.Password = Setting.GetRaw();
        }

        public override FrameworkElement GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener((s) =>
            { _element.Dispatcher.BeginInvoke(new Action(() => { Reset(); })); });
            _element.ToolTip = Setting.Definition.Description;
            Reset();
            return _element;
        }
    }
}
