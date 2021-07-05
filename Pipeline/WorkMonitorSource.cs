using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using System;
using System.IO;
using System.Text;

namespace com.antlersoft.HostedTools.Pipeline
{
    public class WorkMonitorSource : HostedObjectBase, IWorkMonitorSource
    {
        public static IWorkMonitor EmptyMonitor { get; } = new DoNothingMonitor();

        private IWorkMonitor _currentMonitor = EmptyMonitor;

        public IWorkMonitor SetMonitor(IWorkMonitor newMonitor)
        {
            if (newMonitor != null)
            {
                _currentMonitor = newMonitor;
            }
            return _currentMonitor;
        }

        private IWorkMonitor ClearMonitor()
        {
            _currentMonitor = EmptyMonitor;
            return _currentMonitor;
        }

        public IWorkMonitor GetMonitor()
        {
            return _currentMonitor;
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
