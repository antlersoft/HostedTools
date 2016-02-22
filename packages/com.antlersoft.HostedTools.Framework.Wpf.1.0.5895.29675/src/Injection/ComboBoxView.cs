using System;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class ComboBoxView : SettingViewBase
    {
        internal readonly ComboBox _element;

        public ComboBoxView()
        {
            _element = new ComboBox();
            _element.IsEditable = true;
            _element.IsReadOnly = false;
            _element.KeyUp += delegate { CheckForChange(); };
            _element.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
                {
                    if (args.AddedItems.Count > 0)
                    {
                        SetNeedsSave(args.AddedItems[0].ToString() != Setting.GetRaw());
                    }
                };
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.Text);
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _element.Items.Clear();
            foreach (var val in Setting.PreviousValues)
            {
                _element.Items.Add(val);
            }
            _element.Text = Setting.GetRaw();
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
            SetNeedsSave(_element.Text != Setting.GetRaw());
        }
    }
}
