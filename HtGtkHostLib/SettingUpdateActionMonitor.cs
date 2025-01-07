using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using Gtk;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class SettingUpdateActionMonitor : HostedObjectBase, IClearableMonitor
    {
        Window _containingWindow;
        private readonly CancellationToken _token = new CancellationToken();
        private readonly NotifiedTextWriter _writer;
        private readonly StringBuilder _displayString;
        private readonly IWorkMonitorHolder _holder;

        internal SettingUpdateActionMonitor(Window containingWindow, IWorkMonitorHolder holder)
        {
            _containingWindow = containingWindow;
            _writer = new NotifiedTextWriter();
            _holder = holder;
            _displayString = new StringBuilder();
            _writer.OnFlush += s => _displayString.Append(s);
        }

        internal string GetAllOutput()
        {
            return _displayString.ToString();
        }

        internal void RunUpdateAction(Action<IWorkMonitor, ISetting> isa, ISetting setting)
        {
            Clear();
            Thrown = null;
            try
            {
                _holder.SetMonitor(this);
                isa.Invoke(this, setting);
            }
            catch (Exception ex)
            {
                Writer.WriteLine(ex.Message);
                Writer.WriteLine(ex.ToString());
            }
            finally
            {
                _holder.ClearMonitor();
            }
            Writer.Flush();
            string s = GetAllOutput();
            if (s.Length > 0)
            {
                var d=new MessageDialog(_containingWindow, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, s);
                d.Run();
            }
        }
        public void Clear()
        {
            _displayString.Clear();
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }

        public bool IsCanceled
        {
            get { return false; }
        }

        public Exception Thrown { get; set; }

        public CancellationToken Cancellation
        {
            get { return _token; }
        }
    }
}
