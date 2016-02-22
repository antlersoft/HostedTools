using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Plugin;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class SettingUpdateActionMonitor : HostedObjectBase, IClearableMonitor
    {
        private readonly CancellationToken _token = new CancellationToken();
        private readonly NotifiedTextWriter _writer;
        private readonly StringBuilder _displayString;

        internal SettingUpdateActionMonitor()
        {
            _writer = new NotifiedTextWriter();
            _displayString = new StringBuilder();
            _writer.OnFlush += s => _displayString.Append(s);
        }

        internal string GetAllOutput()
        {
            return _displayString.ToString();
        }

        internal void RunUpdateAction(Action<IWorkMonitor,ISetting> isa, ISetting setting)
        {
            Clear();
            Thrown = null;
            try
            {
                isa.Invoke(this, setting);
            }
            catch (Exception ex)
            {
                Writer.WriteLine(ex.Message);
                Writer.WriteLine(ex.ToString());
            }
            Writer.Flush();
            string s = GetAllOutput();
            if (s.Length > 0)
            {
                MessageBox.Show(s);
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
