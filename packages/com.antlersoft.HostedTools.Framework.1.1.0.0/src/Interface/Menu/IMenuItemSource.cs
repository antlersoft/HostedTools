using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Menu
{
    public interface IMenuItemSource
    {
        IEnumerable<IMenuItem> Items { get; } 
    }
}
