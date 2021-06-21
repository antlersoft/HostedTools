using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    [InheritedExport(typeof(IPlugin))]
    [InheritedExport(typeof(ISettingDefinitionSource))]
    [InheritedExport(typeof(IMenuItemSource))]
    public abstract class EditingSettingDeclarer : HostedObjectBase, IPlugin, ISettingDefinitionSource, ISettingEditList, IMenuItemSource
    {
        private List<ISettingDefinition> _definitions;
        private List<IMenuItem> _menuEntries;
 
        protected EditingSettingDeclarer(IEnumerable<IMenuItem> menuEntries, IEnumerable<ISettingDefinition> definitions)
        {
            _menuEntries = menuEntries.ToList();
            _definitions = definitions.ToList();
        }

        public virtual string Name
        {
            get { return GetType().FullName; }
        }

        public virtual IEnumerable<ISettingDefinition> Definitions
        {
            get { return _definitions; }
        }

        public virtual IEnumerable<string> KeysToEdit
        {
            get { return Definitions.Select(d => d.FullKey()); }
        }

        public virtual IEnumerable<IMenuItem> Items
        {
            get { return _menuEntries; }
        }
    }
}
