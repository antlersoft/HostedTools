using System;
using System.Linq;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class EnumView : ComboBoxView
    {
        private bool _enumsSet;

        public override void Reset()
        {
            if (!_enumsSet)
            {
                _element.Sensitive = false;
                foreach (object v in Enum.GetValues(Setting.Definition.Type))
                {
                    _element.AppendText(v.ToString());
                }
                _enumsSet = true;
            }
            _element.Active = Array.IndexOf(Enum.GetValues(Setting.Definition.Type), Setting.GetRaw());
            SetNeedsSave(false);
        }
    }
}
