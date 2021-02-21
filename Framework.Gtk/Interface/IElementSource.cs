using com.antlersoft.HostedTools.Framework.Interface;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Interface
{
    public interface IElementSource : IHostedObject
    {
        Widget GetElement(object container);
    }
}
