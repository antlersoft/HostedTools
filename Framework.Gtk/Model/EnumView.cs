using System;
using System.Linq;
using System.Runtime.Serialization;

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
            object enumValue = null;
            try {
                enumValue = Enum.Parse(Setting.Definition.Type, Setting.GetRaw());                
            } catch (Exception e) {
                // Do nothing
            }
            if (enumValue == null) {
                _element.Active = 0;
            } else {
                _element.Active = Array.IndexOf(Enum.GetValues(Setting.Definition.Type), enumValue);
            }
            SetNeedsSave(false);
        }
    }
}
