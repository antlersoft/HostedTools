using System;
using System.Collections.Generic;
using System.Text;
using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class ImageOutput : Image, IImageOutput
    {
        public void AddImage(Func<Image> source)
        {
            throw new NotImplementedException();
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
