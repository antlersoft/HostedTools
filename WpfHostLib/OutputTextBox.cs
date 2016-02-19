using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using com.antlersoft.HostedTools.Utility;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class OutputTextBox : TextBox, ITextOutput
    {
        private new const int MaxLines = 5000;

        private int _lineCount;
        private readonly Queue<int> _offsets = new Queue<int>(); 

        internal OutputTextBox()
        {
            KeyDown += (sender, args) =>
            {
                if (args.Key == Key.F1)
                {
                    string highlit = SelectedText;
                    long val;
                    if (Int64.TryParse(highlit, out val))
                    {
                        // Values in newtonsoft json are in milliseconds
                        if (val > 10000000000L)
                        {
                            val /= 1000L;
                        }
                        MessageBox.Show(UnixTime.UtcFromUnixTime(val).ToString("o"));
                    }
                }
            };
            IsReadOnly = true;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;            
        }

        public void AddText(string text)
        {
            Dispatcher.Invoke(() =>
                {
                    if (_lineCount == MaxLines)
                    {
                        Text = Text.Substring(_offsets.Dequeue()) + text;
                    }
                    else
                    {
                        Text = Text + text;
                        _lineCount++;
                    }
                    _offsets.Enqueue(text.Length);
                });
        }

        public new void Clear()
        {
            Dispatcher.Invoke(() =>
            {
                _lineCount = 0;
                _offsets.Clear();
                base.Clear();
            });
        }

        public void SetFont(Font font)
        {
            Dispatcher.Invoke(() =>
            {
                FontFamily = new System.Windows.Media.FontFamily(font.Name);
                FontSize = font.Size;
                SetFontStyle(font.Style, font.Underline);
            });
        }

        private void SetFontStyle(System.Drawing.FontStyle fontStyle, bool underline)
        {
            if ((fontStyle & System.Drawing.FontStyle.Regular) == System.Drawing.FontStyle.Regular && !underline)
            {
                FontStyle = FontStyles.Normal;
                FontWeight = FontWeights.Normal;
                TextDecorations.Clear();
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

            if ((fontStyle & System.Drawing.FontStyle.Strikeout) == System.Drawing.FontStyle.Strikeout)
            {
                TextDecorations.Add(System.Windows.TextDecorations.Strikethrough);
            }

            if (underline)
            {
                TextDecorations.Add(System.Windows.TextDecorations.Underline);
            }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
