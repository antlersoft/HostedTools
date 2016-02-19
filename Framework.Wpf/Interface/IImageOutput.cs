using System;
using System.Windows.Media;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Framework.Wpf.Interface
{
    public interface IImageOutput : IHostedObject
    {
        void AddImage(Func<ImageSource> source);
        void Clear();
    }
}
