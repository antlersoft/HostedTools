using System;
using System.Collections.Generic;
using System.Text;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class OutputListBox : TreeView, ITextOutput
    {
        ListStore _model = new ListStore(new Type[] { typeof(string) });
        internal OutputListBox()
        {
            this.Model = new ListStore(new Type[] { typeof(string) });
        }
        public void AddText(string text)
        {
            _model.AppendValues(new object[] { text });
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public void Clear()
        {
            _model.Clear();
        }

        public void SetFont(object font)
        {
            // throw new NotImplementedException();
        }
    }
}
