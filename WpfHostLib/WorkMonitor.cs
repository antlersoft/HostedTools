using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using com.antlersoft.HostedTools.Utility;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Plugin;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    internal class WorkMonitor : HostedObjectBase, IClearableMonitor, IBackgroundableMonitor, ICancelableMonitor, IElementSource
    {
        private readonly NotifiedTextWriter _writer = new NotifiedTextWriter();
        private readonly OutputPane _outputPane;
        private readonly ITextOutput _textOutput;
        private readonly FrameworkElement _element;
        private readonly IWork _work;
        private CancellationTokenSource _source;
        private bool _isCanceled;

        private Action _notifyBackgroundChanged;
        private Action _notifyRunningChanged;

        internal WorkMonitor(IWork work)
        {
            _work = work;
            _outputPane = new OutputPane(work.Cast<IOutputPaneList>());
            _element = _outputPane.Element;
            InjectImplementation(typeof (IHasOutputPanes), _outputPane);
            InjectImplementation(typeof (IHasImageOutput), _outputPane);
            _textOutput = _outputPane.FindTextOutput();
            if (_textOutput != null)
            {
                _writer.OnFlush += str =>
                    {
                        _textOutput.AddText(str);
                    };
            }
            IGridOutput grid = _outputPane.FindGridOutput();
            if (grid != null)
            {
                var listenerCollection = grid.Cast<IListenerCollection<Dictionary<string, object>>>();
                if (listenerCollection != null)
                {
                    listenerCollection.AddListener(row =>
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var key in row.OrderBy(k => k.Key))
                        {
                            if (sb.Length != 0)
                            {
                                sb.Append(',');
                            }
                            sb.Append(key.Value.ToString());
                        }
                        _writer.WriteLine(sb.ToString());
                    });
                }
            }
            CanPutInBackgroundChanged = new ListenerCollection<bool>();
            IsRunningChanged = new ListenerCollection<bool>();
            _notifyBackgroundChanged = () => ((ListenerCollection<bool>)CanPutInBackgroundChanged).NotifyListeners(CanPutInBackground);
            _notifyRunningChanged = () => ((ListenerCollection<bool>)IsRunningChanged).NotifyListeners(IsRunning);
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }

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

        public Exception Thrown { get; set; }

        public bool IsRunning { get; private set; }

        public IListenerCollection<bool> IsRunningChanged
        {
            get;
            private set;
        }

        public bool CanPutInBackground { get; private set; }

        public string BackgroundTitle { get; private set; }

        public IListenerCollection<bool> CanPutInBackgroundChanged { get; private set; }

        /// <summary>
        /// Must call on UI thread
        /// </summary>
        /// <returns></returns>
        public async void RunWork()
        {
            Clear();
            IsCanceled = false;
            _source = null;
            Thrown = null;
            if (IsRunning)
            {
                return;
            }
            IsRunning = true;
            _notifyRunningChanged.Invoke();
            await Task.Run(() =>
                {
                    try
                    {
                        _work.Perform(this);
                    }
                    catch (Exception ex)
                    {
                        Writer.WriteLine(ex.ToString());
                        IsCanceled = true;
                    }
                });
            IsRunning = false;
            _element.Dispatcher.Invoke(_notifyRunningChanged);
        }

        public void Clear()
        {
            if (_textOutput != null)
            {
                _textOutput.Clear();
            }
        }

        public void CanBackground(string backgroundTitle)
        {
            CanPutInBackground = true;
            if (backgroundTitle != null)
            {
                BackgroundTitle = backgroundTitle;
            }
            _outputPane.Element.Dispatcher.Invoke(_notifyBackgroundChanged);
        }

        public FrameworkElement GetElement(object container)
        {
            return _outputPane.Element;
        }

        public CancellationToken Cancellation
        {
            get
            {
                if (_source == null)
                {
                    _source = new CancellationTokenSource();
                    if (IsCanceled && IsRunning)
                    {
                        _source.Cancel();
                    }
                }
                return _source.Token;
            }
        }
    }
}
