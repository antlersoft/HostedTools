using System;
using System.Linq;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class EnumView : ComboBoxView
    {
        public EnumView()
        : base(false)
        {

        }

        private bool _enumsSet;

        public override void Reset()
        {
            if (!_enumsSet)
            {
                foreach (object v in Enum.GetValues(Setting.Definition.Type))
                {
                    _element.AppendText(v.ToString());
                }
                _enumsSet = true;
            }
            _element.Active = Array.IndexOf(Enum.GetValues(Setting.Definition.Type), Enum.Parse(Setting.Definition.Type, Setting.GetRaw()));
            SetNeedsSave(false);
        }
    }
}
