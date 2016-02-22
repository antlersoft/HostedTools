using System.Windows;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Framework.Wpf.Interface
{
    public interface IElementSource : IHostedObject
    {
        FrameworkElement GetElement(object container);
    }
}
