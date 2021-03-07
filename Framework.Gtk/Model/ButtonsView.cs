using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class ButtonsView : SettingViewBase
    {
        private readonly HBox _grid;

        public ButtonsView()
        {
            _grid = new HBox();
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
                    foreach (string id in ba.ButtonIdentifiers)
                    {
                        string[] splits = id.Split('|');
                        if (splits.Length == 0)
                        {
                            continue;
                        }
                        string buttonId = splits[0];
                        string buttonLabel = splits.Length > 1 ? splits[1] : buttonId;
                        Button button = new Button() { Label = buttonLabel };
                        button.Clicked += (obj, args) => Setting.SetRaw(buttonId);
                        _grid.PackStart(button, false, false, 0);
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

        public override Widget GetElement(object container)
        {
            return _grid;
        }
    }
}
