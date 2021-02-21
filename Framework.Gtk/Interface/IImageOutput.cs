using System;
using Gtk;

using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Framework.Gtk.Interface
{
    public interface IImageOutput : IHostedObject
    {
        void AddImage(Func<Image> source);
        void Clear();
    }
}
