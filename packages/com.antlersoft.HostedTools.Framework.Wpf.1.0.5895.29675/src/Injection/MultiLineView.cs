using System;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class MultiLineView : SettingViewBase
    {
        private DockPanel _multiViewPanel;
        private TextBox _textBox;
        private Button _upButton;
        private Button _downButton;
        private int _currentPosition;

        public MultiLineView()
        {
            _textBox = new TextBox();
            _textBox.AcceptsReturn = true;
            _textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            _textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            _textBox.TextChanged += (obj, args) => SetNeedsSave(true);
            _multiViewPanel = new DockPanel();
            var buttonPanel = new DockPanel();
            _upButton = new Button() {Content = "^"};
            _upButton.Click += MoveUp;
            _downButton = new Button() {Content = "v"};
            _downButton.Click += MoveDown;
            DockPanel.SetDock(_upButton, Dock.Top);
            DockPanel.SetDock(_downButton, Dock.Bottom);
            buttonPanel.Children.Add(_upButton);
            buttonPanel.Children.Add(_downButton);
            DockPanel.SetDock(buttonPanel, Dock.Left);
            _multiViewPanel.Children.Add(buttonPanel);
            _multiViewPanel.Children.Add(_textBox);
        }

        internal void MoveDown(object sender, RoutedEventArgs args)
        {
            if (_currentPosition < Setting.PreviousValues.Count)
            {
                _textBox.Text = Setting.PreviousValues[_currentPosition];
                _currentPosition += 1;
                _downButton.IsEnabled = _currentPosition < Setting.PreviousValues.Count;
                _upButton.IsEnabled = true;
                SetNeedsSave(true);
            }
        }

        internal void MoveUp(object sender, RoutedEventArgs args)
        {
            if (_currentPosition > 0)
            {
                _currentPosition -= 1;
                _textBox.Text = Setting.PreviousValues[_currentPosition];
                _upButton.IsEnabled = _currentPosition > 0;
                _downButton.IsEnabled = true;
                SetNeedsSave(true);
            }
        }

        internal override ISetting Setting
        {
            set
            {
                base.Setting = value;
                IMultiLineValue mlv = value.Definition.Cast<IMultiLineValue>();
                if (mlv != null)
                {
                    _textBox.MinLines = mlv.DesiredVisibleLinesOfText;
                    _textBox.MaxLines = mlv.DesiredVisibleLinesOfText;
                }
                Setting.SettingChangedListeners.AddListener(s =>
                {
                    string text = s.GetRaw();
                    _textBox.Text = text;
                });
                _textBox.Text = Setting.GetRaw();
            }
        }

        public override bool TrySave()
        {
            string currentText = _textBox.Text;
            Setting.SetRaw(currentText);
            _upButton.IsEnabled = false;
            _downButton.IsEnabled = Setting.PreviousValues.Count > 0;
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _currentPosition = 0;
            _textBox.Text = Setting.GetRaw();
            _upButton.IsEnabled = false;
            _downButton.IsEnabled = Setting.PreviousValues.Count > 0;
            SetNeedsSave(false);
        }

        public override FrameworkElement GetElement(object container)
        {
            return _multiViewPanel;
        }
    }
}
