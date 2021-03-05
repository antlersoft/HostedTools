using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using Gtk;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    public partial class WorkControl : IElementSource
    {
        private readonly IWork _work;
        private WorkMonitor _monitor;
        private readonly Button _cancelButton = new Button() { Label = "Cancel" };
        private readonly Button _runButton = new Button() { Label = "Run" };
        private readonly Table _table = new Table(3, 2, false);
        private readonly Label _explanationLabel = new Label();
        private readonly ISavable _savable;
        private readonly Action<bool> _runningChangedListener;
        private readonly Action<bool> _cancelableChangedListener;
        private Widget _workText;
        private readonly IBackgroundWorkReceiver _backgroundWorkReceiver;

        public WorkControl(IBackgroundWorkReceiver receiver, IWork work, ISavable savable)
        {
            _work = work;
            _backgroundWorkReceiver = receiver;
            _savable = savable;

            // Setup layout
            _table.Attach(_explanationLabel, 0, 2, 0, 1, AttachOptions.Fill, 0, 0, 0);
            _table.Attach(_runButton, 0, 1, 1, 2, 0, 0, 0, 0);
            _table.Attach(_cancelButton, 1, 2, 1, 2, 0, 0, 0, 0);


            _cancelButton.Sensitive = false;
            _cancelButton.Clicked += OnCancel;
            _runButton.Clicked += OnRun;
            _runningChangedListener = b =>
            {
                if (!b)
                {
                    _cancelButton.Sensitive = false;
                    _runButton.Sensitive = true;
                    _runButton.Label = "Run";
                }
            };
            _cancelableChangedListener = b =>
            {
                if (b)
                {
                    _runButton.Label = "Background";
                    _runButton.Sensitive = true;
                }
            };
            InitializeMonitor();
            ShowExplanation(work);
        }

        private void ShowExplanation(IWork work)
        {
            IExplanation explanation = work.Cast<IExplanation>();
            if (explanation != null)
            {
                _explanationLabel.Text = explanation.Explanation;
            }
        }

        private void InitializeMonitor()
        {
            if (_workText != null)
            {
                _table.Remove(_workText);
            }
            _monitor = new WorkMonitor(_work);
            var workText = _monitor.GetElement(this);
            _workText = workText;
            _table.Attach(workText, 0, 2, 2, 3, AttachOptions.Fill, AttachOptions.Expand, 0, 0);
            _monitor.IsRunningChanged.AddListener(_runningChangedListener);
            _monitor.CanPutInBackgroundChanged.AddListener(_cancelableChangedListener);
        }

        private void OnRun(object sender, EventArgs args)
        {
            if (_monitor.IsRunning && _monitor.CanPutInBackground)
            {
                _monitor.IsRunningChanged.RemoveListener(_runningChangedListener);
                _monitor.CanPutInBackgroundChanged.RemoveListener(_cancelableChangedListener);
                _cancelButton.Sensitive = false;
                _runButton.Label = "Run";
                _table.Remove(_workText);
                _backgroundWorkReceiver.AcceptWork(_monitor, _workText, _monitor.BackgroundTitle);
                InitializeMonitor();
            }
            else
            {
                if (_savable != null)
                {
                    if (_savable.NeedsSave())
                    {
                        if (!_savable.TrySave())
                        {
                            return;
                        }
                    }
                }
                _runButton.Sensitive = false;
                _cancelButton.Sensitive = true;
                _monitor.RunWork();
            }
        }

        private void OnCancel(object sender, EventArgs args)
        {
            _monitor.IsCanceled = true;
            _cancelButton.Sensitive = false;
        }

        public Widget GetElement(object container)
        {
            return _table;
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
