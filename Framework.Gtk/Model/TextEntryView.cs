
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class TextEntryView : SettingViewBase
    {
        internal readonly Entry _element;

        public TextEntryView()
        {
            _element = new Entry();
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
