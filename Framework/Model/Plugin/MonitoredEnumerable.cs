using System;
using System.Collections.Generic;

using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    public class MonitoredEnumerable<T> : CountingEnumerable<T>
    {
        private class MonitoredEnumerator : IEnumerator<T>
        {
            private IWorkMonitor _monitor;
            private IEnumerator<T> _base;
            private bool _monitorEnd;

            internal MonitoredEnumerator(IEnumerator<T> b, IWorkMonitor monitor)
            {
                _base = b;
                _monitor = monitor;
            }

            public T Current
            {
                get
                {
                    if (_monitorEnd)
                    {
                        throw new InvalidOperationException("Move past end of canceled MonitoredEnumerable");
                    }
                    return _base.Current;
                }
            }

            public void Dispose()
            {
                _base.Dispose();
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
				var cancelable = _monitor as ICancelableMonitor;
                if (cancelable != null && cancelable.IsCanceled)
                {
                    _monitorEnd = true;
                    return false;
                }
                return _base.MoveNext();
            }

            public void Reset()
            {
                _monitorEnd = false;
                _base.Reset();
            }
        }
        private readonly string _format;
        private readonly bool _clear;
        private readonly IClearableMonitor _clearable;
        private readonly IWorkMonitor _monitor;
        public MonitoredEnumerable(IEnumerable<T> baseEnumerable, IWorkMonitor monitor, int stepSize = 100,
            string format = "{0}", bool clear = true)
            : base(baseEnumerable, stepSize)
        {
            _format = format;
            _clear = clear;
            _clearable = monitor.Cast<IClearableMonitor>();
            _monitor = monitor;
        }

        protected override void WhenCounted(int count)
        {
            if (_clear && _clearable != null)
            {
                _clearable.Clear();
            }
            _monitor.Writer.WriteLine(_format, count);
        }

        protected override IEnumerator<T> ConstructEnumerator()
        {
            return new MonitoredEnumerator(base.ConstructEnumerator(), _monitor);
        }
    }
}
