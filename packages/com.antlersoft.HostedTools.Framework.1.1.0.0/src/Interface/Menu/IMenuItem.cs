using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Menu
{
    public interface IMenuItem : IHostedObject
    {
        string Key { get; }
        string Prompt { get; }
        string ParentKey { get; }
        string ActionId { get; }
    }
}
