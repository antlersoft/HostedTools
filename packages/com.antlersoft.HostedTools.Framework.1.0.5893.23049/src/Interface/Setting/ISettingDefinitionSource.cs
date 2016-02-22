using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISettingDefinitionSource
    {
        IEnumerable<ISettingDefinition> Definitions { get; } 
    }
}
