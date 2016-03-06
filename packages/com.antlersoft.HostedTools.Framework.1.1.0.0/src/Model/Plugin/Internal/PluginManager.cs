using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin.Internal
{
    [Export(typeof(IPluginManager))]
    [Export(typeof(IAfterComposition))]
    public class PluginManager : HostedObjectBase, IPluginManager, IAfterComposition
    {
        [ImportMany]
        public IEnumerable<IPlugin> Imported;

        [ImportMany] public IEnumerable<IPluginSource> ImportedSources;

        private List<IPlugin> _allPlugins = new List<IPlugin>();  
 
        public IPlugin this[string key]
        {
            get
            {
                return _allPlugins.FirstOrDefault(p => p.Name == key);
            }
        }

        public IEnumerable<IPlugin> Plugins
        {
            get { return _allPlugins; }
        }

        public void AfterComposition()
        {
            _allPlugins.AddRange(Imported);
            foreach (var ps in ImportedSources)
            {
                _allPlugins.AddRange(ps.SourcePlugins);
            }
        }
    }
}
