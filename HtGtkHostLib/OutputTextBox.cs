using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class OutputTextBox : ScrolledWindow, ITextOutput
    {
        private const int MaxLines = 5000;

        private int _lineCount;
        private TextView _text = new TextView();
        private readonly Queue<int> _offsets = new Queue<int>();

        internal OutputTextBox()
        {
            _text.Editable = false;
            // Enable scrollbars
            Add(_text);
            // Enable display of highlighted unix time
        }

        public void AddText(string text)
        {
            Gtk.Application.Invoke(delegate { InternalAddText(text); });
        }

        private void InternalAddText(string text)
        {
            var buffer = _text.Buffer;
            if (buffer == null)
            {
                buffer = new TextBuffer(new TextTagTable());
                buffer.Text = text;
                _lineCount = 1;
                _text.Buffer = buffer;
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
                if (_text.Buffer != null)
                {
                    _text.Buffer.Text = string.Empty;
                }
            });
        }

        public void SetFont(object font)
        {
            // throw new NotImplementedException();
        }
    }
}
