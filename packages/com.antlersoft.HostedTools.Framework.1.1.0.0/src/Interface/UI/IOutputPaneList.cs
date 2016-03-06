using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface IOutputPaneList : IHostedObject
    {
        EPaneListOrientation Orientation { get; }
        IList<IOutputPaneSpecifier> Panes { get; } 
    }
}
