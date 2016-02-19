using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface IButtonArray
    {
        // Return list of button identifiers|label separated by a pipe character
        // If there is no pipe character, the identifier is used as the label
        IEnumerable<string> ButtonIdentifiers { get; }
    }
}
