using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface IOutputPaneSpecifier : IHostedObject
    {
        EOutputPaneType Type { get; }
        string Title { get; }
        int Proportion { get; }
        IOutputPaneList NestedPanes { get; }
    }
}
