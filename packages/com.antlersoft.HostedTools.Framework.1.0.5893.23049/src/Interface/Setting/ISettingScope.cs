using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface ISettingScope : IHostedObject
    {
        string ScopeKey { get; }
        ISetting this[string name] { get; }
        IEnumerable<ISetting> Settings { get; } 
    }
}
