
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class TextEntryView : SettingViewBase
    {
        internal readonly Entry _element;

        internal override ISetting Setting {
            set {
                base.Setting = value;
                if (Setting.Definition.Cast<IReadOnly>() is IReadOnly ro) {
                    _element.Sensitive = ! ro.IsReadOnly();
                    ro.ReadOnlyChangeListeners.AddListener(
                        (a) => { Application.Invoke(delegate {
                            _element.Sensitive = ! a.IsReadOnly();
                        }); }
                    );
                }
            }
        }

        public TextEntryView()
        {
            _element = new Entry();
            _element.PopulatePopup += (object source, PopulatePopupArgs args) => {
                if (args.Popup is Menu menu)
                {
                    foreach (var item in GetPopupMenuItems())
                    {
                        item.Visible = true;
                        menu.Add(item);
                    }
                }
            };
            SetNeedsSave(true);
        }

        public override bool TrySave()
        {
            Setting.SetRaw(_element.Text);
            return true;
        }

        public override void Reset()
        {
            _element.Text = Setting.GetRaw();
        }

        public override Widget GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener((s) =>
            {Application.Invoke(delegate { Reset(); }) ; });
            _element.TooltipText = Setting.Definition.Description;
            Reset();
            return _element;
        }
    }
}
