using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin.Internal
{
    [Export(typeof(IPluginManager))]
    public class PluginManager : IPluginManager
    {
        [ImportMany]
        public IEnumerable<IPlugin> Imported;
 
        public IPlugin this[string key]
        {
            get
            {
                return Imported.FirstOrDefault(p => p.Name == key);
            }
        }

        public IEnumerable<IPlugin> Plugins
        {
            get { return Imported; }
        }
    }
}
