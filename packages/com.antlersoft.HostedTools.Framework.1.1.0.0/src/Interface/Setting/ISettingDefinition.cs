using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISettingDefinition : IAggregator
    {
        Type Type { get; }
        bool UseExpansion { get; }
        string ScopeKey { get; }
        string Name { get; }
        string Prompt { get; }
        string Description { get; }
        string DefaultRaw { get; }
        int NumberOfPreviousValues { get; }
    }
}
