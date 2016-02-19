
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class ButtonsView : SettingViewBase
    {
        private readonly Grid _grid;

        public ButtonsView()
        {
            _grid = new Grid();    
        }

        internal override ISetting Setting
        {
            set
            {
                base.Setting = value;
                ISettingDefinition sd = value.Definition;
                IButtonArray ba = sd.Cast<IButtonArray>();
                if (ba != null)
                {
                    int count = 0;
                    foreach (string id in ba.ButtonIdentifiers)
                    {
                        string[] splits = id.Split('|');
                        if (splits.Length == 0)
                        {
                            continue;
                        }
                        string buttonId = splits[0];
                        string buttonLabel = splits.Length > 1 ? splits[1] : buttonId;
                        Button button = new Button() {Content = buttonLabel};
                        button.Click += (obj, args) => Setting.SetRaw(buttonId);
                        Grid.SetRow(button,  0);
                        Grid.SetColumn(button, count++);
                        _grid.Children.Add(button);
                    }
                    for (; count > 0; --count)
                    {
                        _grid.ColumnDefinitions.Add(new ColumnDefinition() {Width = GridLength.Auto});
                    }
                }
            }
        }

        public override bool TrySave()
        {
            return true;
        }

        public override void Reset()
        {
        }

        public override FrameworkElement GetElement(object container)
        {
            return _grid;
        }
    }
}
