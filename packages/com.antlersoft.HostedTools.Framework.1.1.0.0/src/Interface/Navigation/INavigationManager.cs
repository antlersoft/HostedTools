using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Navigation
{
    public interface INavigationManager
    {
        string CurrentLocation { get; }
        void NavigateTo(string destination);
        void GoBack();
        void GoForward();
        IList<string> History { get; }
        IList<string> Forward { get; }
        IListenerCollection<INavigationManager> NavigationListeners { get; } 
    }
}
