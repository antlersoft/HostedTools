using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public static class SettingDefinitionExtension
    {
        public static string FullKey(this ISettingDefinition item)
        {
            return item.ScopeKey + "." + item.Name;
        }

        public static T Value<T>(this ISettingDefinition definition, ISettingManager manager)
        {
            return manager.Scope(definition.ScopeKey)[definition.Name].Get<T>();
        }
    }
}
