using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Threading;

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

        class DoNothingMonitor : HostedObjectBase, ICancelableMonitor
        {
            DoNothingWriter writer = new DoNothingWriter();
            public TextWriter Writer => writer;
            private bool _isCanceled = false;
            private CancellationTokenSource _source;

            public Exception Thrown { get; set; }
            public bool IsCanceled
            {
                get { return _isCanceled; }
                set
                {
                    if (!_isCanceled && value)
                    {
                        if (_source == null)
                        {
                            _isCanceled = true;
                        }
                        else
                        {
                            _source.Cancel();
                            _isCanceled = true;
                        }
                    }
                    else
                    {
                        _isCanceled = value;
                    }
                }
            }
            
            public CancellationToken Cancellation
            {
                get
                {
                    if (_source == null)
                    {
                        _source = new CancellationTokenSource();
                        if (IsCanceled)
                        {
                            _source.Cancel();
                        }
                    }
                    return _source.Token;
                }
            }
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
