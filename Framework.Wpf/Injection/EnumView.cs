using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class EnumView: ComboBoxView
    {
        private bool _enumsSet;

        public override void Reset()
        {
            if (! _enumsSet)
            {
                _element.IsEditable = false;
                foreach (object v in Enum.GetValues(Setting.Definition.Type))
                {
                    _element.Items.Add(v.ToString());
                }
                _enumsSet = true;
            }
            _element.Text = Setting.GetRaw();
            SetNeedsSave(false);
        }
    }
}
