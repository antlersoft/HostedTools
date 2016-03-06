using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    [InheritedExport(typeof(IPlugin))]
    [InheritedExport(typeof(IMenuItemSource))]
    public abstract class EditOnlyPlugin : HostedObjectBase, IPlugin, ISettingEditList, IMenuItemSource
    {
        [Import]
        public ISettingManager SettingManager { get; set; }

        private List<IMenuItem> _menuEntries;
        private List<string> _keys; 
 
        protected EditOnlyPlugin(IEnumerable<IMenuItem> menuEntries, IEnumerable<string> keys)
        {
            _menuEntries = menuEntries.ToList();
            _keys = keys.ToList();
        }

        protected EditOnlyPlugin(IMenuItem item, IEnumerable<string> keys)
            : this(new[] {item}, keys)
        {
            
        }

        public virtual string Name
        {
            get { return GetType().FullName; }
        }

        public virtual IEnumerable<IMenuItem> Items
        {
            get { return _menuEntries; }
        }

        public virtual IEnumerable<string> KeysToEdit
        {
            get { return _keys; }
        }
    }
}
