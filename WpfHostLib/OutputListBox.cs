﻿
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class OutputListBox : ListBox, ITextOutput
    {
        public new void AddText(string text)
        {
            if (text.EndsWith("\r\n"))
            {
                text = text.Substring(0, text.Length - 2);
            }
            LambdaDispatch.Invoke(Dispatcher, () => Items.Add(text));
        }

        public void Clear()
        {
            LambdaDispatch.Invoke(Dispatcher, () => Items.Clear());
        }

        public void SetFont(object fontObj)
        {
            Font font = fontObj as Font;
            if (font == null)
            {
                return;
            }
            LambdaDispatch.Invoke(Dispatcher, () =>
            {
                FontFamily = new System.Windows.Media.FontFamily(font.Name);
                FontSize = font.Size;
                SetFontStyle(font.Style);
            });
        }

        private void SetFontStyle(System.Drawing.FontStyle fontStyle)
        {
            if ((fontStyle & System.Drawing.FontStyle.Regular) == System.Drawing.FontStyle.Regular)
            {
                FontStyle = FontStyles.Normal;
                FontWeight = FontWeights.Normal;
                return;
            }

            if ((fontStyle & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
            {
                FontStyle = FontStyles.Italic;
            }

            if ((fontStyle & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
            {
                FontWeight = FontWeights.Bold;
            }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
