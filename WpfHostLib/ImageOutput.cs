using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    internal class ImageOutput : Image, IImageOutput
    {
        internal ImageOutput()
        {
        }

        public void AddImage(Func<ImageSource> sourceFunctor)
        {
            if (sourceFunctor != null)
            {
                Dispatcher.Invoke(() =>
                {
                    ImageSource imageSource = sourceFunctor();
                    Source = imageSource;
                });
            }
        }

        public void Clear()
        {
            Dispatcher.Invoke(() =>
            {
                //_imageSources.Clear();
            });
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
