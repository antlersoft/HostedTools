using System;
using System.Collections.Generic;
using System.Text;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class OutputListBox : ScrolledWindow, ITextOutput
    {
        TreeView tree = new TreeView();
        ListStore _model = new ListStore(new Type[] { typeof(string) });
        internal OutputListBox()
        {
            tree.Model = new ListStore(new Type[] { typeof(string) });
            Add(tree);
        }
        public void AddText(string text)
        {
            Gtk.Application.Invoke(delegate { _model.AppendValues(new object[] { text }); }) ;
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public void Clear()
        {
            LambdaDispatch.Invoke(() => _model.Clear());
        }

        public void SetFont(object font)
        {
            // throw new NotImplementedException();
        }
    }
}
