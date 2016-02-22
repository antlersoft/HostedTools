using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface IHasOutputPanes : IHostedObject
    {
        ITextOutput FindTextOutput(string title = null);
        IGridOutput FindGridOutput(string title = null);
    }
}
