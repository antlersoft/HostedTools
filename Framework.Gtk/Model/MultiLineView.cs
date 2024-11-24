using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using Gtk;
using System;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class MultiLineView : SettingViewBase
    {
        private HBox _multiViewPanel;
        private ScrolledWindow _scrolledWindow;
        private TextView _textBox;
        private TextBuffer _buffer;
        private Button _upButton;
        private Button _downButton;
        private int _currentPosition;

        public MultiLineView()
        {
            _scrolledWindow = new ScrolledWindow();
            _buffer = new TextBuffer(new TextTagTable());
            _textBox = new TextView(_buffer);
            _textBox.PopulatePopup += (object source, PopulatePopupArgs args) => {
                if (args.Popup is Menu menu)
                {
                    foreach (var item in GetPopupMenuItems())
                    {
                        item.Visible = true;
                        menu.Add(item);
                    }
                }
            };
            _scrolledWindow.Add(_textBox);
            //_textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            // _textBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            _buffer.Changed += (obj, args) => SetNeedsSave(true);
            _multiViewPanel = new HBox();
            var buttonPanel = new VBox();
            _upButton = new Button() { Label = "^" };
            _upButton.Clicked += MoveUp;
            _downButton = new Button() { Label = "v" };
            _downButton.Clicked += MoveDown;
            buttonPanel.PackStart(_upButton, false, true, 0);
            buttonPanel.PackEnd(_downButton, false, true, 0);
            _multiViewPanel.PackStart(buttonPanel, false, true, 0);
            _multiViewPanel.PackEnd(_scrolledWindow, true, true, 0);
        }

        internal void MoveDown(object sender, EventArgs args)
        {
            if (_currentPosition < Setting.PreviousValues.Count)
            {
                _buffer.Text = Setting.PreviousValues[_currentPosition];
                _currentPosition += 1;
                _downButton.Sensitive = _currentPosition < Setting.PreviousValues.Count;
                _upButton.Sensitive = true;
                SetNeedsSave(true);
            }
        }

        internal void MoveUp(object sender, EventArgs args)
        {
            if (_currentPosition > 0)
            {
                _currentPosition -= 1;
                _buffer.Text = Setting.PreviousValues[_currentPosition];
                _upButton.Sensitive = _currentPosition > 0;
                _downButton.Sensitive = true;
                SetNeedsSave(true);
            }
        }

        internal override ISetting Setting
        {
            set
            {
                base.Setting = value;
                Setting.SettingChangedListeners.AddListener(s =>
                {
                    string text = s.GetRaw();
                    _buffer.Text = text;
                });
                _buffer.Text = Setting.GetRaw();
                if (Setting.Definition.Cast<IReadOnly>() is IReadOnly ro) {
                    _textBox.Sensitive = ! ro.IsReadOnly();
                    ro.ReadOnlyChangeListeners.AddListener( (a) => {
                        Application.Invoke(delegate { _textBox.Sensitive = ! a.IsReadOnly(); });
                    });
                }
                IMultiLineValue mlv = value.Definition.Cast<IMultiLineValue>();
                if (mlv != null && mlv.DesiredVisibleLinesOfText > 0)
                {
                    //_textBox.MinLines = mlv.DesiredVisibleLinesOfText;
                    //_textBox.MaxLines = mlv.DesiredVisibleLinesOfText;
                    _scrolledWindow.MinContentHeight = 24 * mlv.DesiredVisibleLinesOfText;
                }
            }
        }

        public override bool TrySave()
        {
            string currentText = _buffer.Text;
            Setting.SetRaw(currentText);
            _upButton.Sensitive = false;
            _downButton.Sensitive = Setting.PreviousValues.Count > 0;
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _currentPosition = 0;
            _buffer.Text = Setting.GetRaw();
            _upButton.Sensitive = false;
            _downButton.Sensitive = Setting.PreviousValues.Count > 0;
            SetNeedsSave(false);
        }

        public override Widget GetElement(object container)
        {
            return _multiViewPanel;
        }
    }
}
