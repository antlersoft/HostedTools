using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class OutputTextBox : TextView, ITextOutput
    {
        private const int MaxLines = 5000;

        private int _lineCount;
        private readonly Queue<int> _offsets = new Queue<int>();

        internal OutputTextBox()
        {
            this.Editable = false;
            // Enable scrollbars
            // Enable display of highlighted unix time
        }

        public void AddText(string text)
        {
            Gtk.Application.Invoke(delegate { InternalAddText(text); });
        }

        private void InternalAddText(string text)
        {
            var buffer = Buffer;
            if (buffer == null)
            {
                buffer = new TextBuffer(new TextTagTable());
                buffer.Text = text;
                _lineCount = 1;
                Buffer = buffer;
                _offsets.Enqueue(text.Length);
                return;
            }
            if (_lineCount == MaxLines)
            {
                buffer.Text = buffer.Text.Substring(_offsets.Dequeue()) + text;
            }
            else
            {
                buffer.Text = buffer.Text + text;
                _lineCount++;
            }
            _offsets.Enqueue(text.Length);
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public void Clear()
        {
            Gtk.Application.Invoke(delegate
            {
                _lineCount = 0;
                _offsets.Clear();
                if (Buffer != null)
                {
                    Buffer.Text = string.Empty;
                }
            });
        }

        public void SetFont(object font)
        {
            // throw new NotImplementedException();
        }
    }
}
