using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export(typeof(ISettingDefinitionSource))]
    public class EnvQuery : SimpleWorker, ISettingDefinitionSource
    {
        public static readonly ISettingDefinition QuerySetting = new SimpleSettingDefinition("SettingToCheck", "", "Setting to Check",
                                                                             "Put in curly braces {}");

        public EnvQuery()
            : base(new[] {new MenuItem("Common.CheckSetting", "Check Setting", typeof (EnvQuery).FullName, "Common")},
                   new [] {QuerySetting.FullKey()})
        {
            
        }

        public IEnumerable<ISettingDefinition> Definitions
        {
            get { return new[] {QuerySetting}; }
        }

        public override void Perform(IWorkMonitor monitor)
        {
            monitor.Writer.WriteLine(QuerySetting.Value<string>(SettingManager));
        }
    }
}
