using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Model.Setting
{
    public class ButtonsDefinition : SimpleSettingDefinition, IButtonArray
    {
        private string[] _buttonIdentifiers;

        public ButtonsDefinition(string name, string scopeKey, IEnumerable<string> buttonIdentifiers, string prompt="", string description = null)
            : base(name, scopeKey, prompt, description, typeof(string), null, true, 0)
        {
            _buttonIdentifiers = buttonIdentifiers.ToArray();
        }

        public IEnumerable<string> ButtonIdentifiers
        {
            get { return _buttonIdentifiers; }
        }
    }
}
