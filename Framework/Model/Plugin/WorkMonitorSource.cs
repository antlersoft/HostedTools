using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;

namespace com.antlersoft.HostedTools.Model.Plugin
{
    [Export(typeof(IWorkMonitorSource))]
    [Export(typeof(IWorkMonitorHolder))]
    public class WorkMonitorSource : HostedObjectBase, IWorkMonitorSource, IWorkMonitorHolder
    {
        public static IWorkMonitor EmptyMonitor { get; } = new DoNothingMonitor();

        private IWorkMonitor _currentMonitor = EmptyMonitor;
        private object _lock = new object();

        public IWorkMonitor SetMonitor(IWorkMonitor newMonitor)
        {
            lock (_lock) {
                if (newMonitor != null)
                {
                    _currentMonitor = newMonitor;
                }
                return _currentMonitor;
            }
        }

        public IWorkMonitor ClearMonitor()
        {
            lock (_lock) {
                _currentMonitor = EmptyMonitor;
                return _currentMonitor;
            }
        }

        public IWorkMonitor GetMonitor()
        {
            lock (_lock) {
                return _currentMonitor;
            }
        }

        class DoNothingMonitor : HostedObjectBase, IWorkMonitor
        {
            DoNothingWriter writer = new DoNothingWriter();
            public TextWriter Writer => writer;

            public Exception Thrown { get; set; }
        }

        class DoNothingWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.Default;

            public override void Write(char c)
            {
            }
        }
    }
}
