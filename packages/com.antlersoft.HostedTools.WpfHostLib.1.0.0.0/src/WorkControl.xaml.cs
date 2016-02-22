using System;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    /// <summary>
    /// Interaction logic for WorkControl.xaml
    /// </summary>
    public partial class WorkControl : IElementSource
    {
        private readonly IWork _work;
        private WorkMonitor _monitor;
        private readonly ISavable _savable;
        private readonly Action<bool> _runningChangedListener;
        private readonly Action<bool> _cancelableChangedListener;
        private FrameworkElement _workText;
        private readonly IBackgroundWorkReceiver _backgroundWorkReceiver;

        public WorkControl(IBackgroundWorkReceiver receiver, IWork work, ISavable savable)
        {
            InitializeComponent();
            _work = work;
            _backgroundWorkReceiver = receiver;
            _savable = savable;
            CancelButton.IsEnabled = false;
            CancelButton.Click += OnCancel;
            RunButton.Click += OnRun;
            _runningChangedListener = b =>
                {
                    if (! b)
                    {
                        CancelButton.IsEnabled = false;
                        RunButton.IsEnabled = true;
                        RunButton.Content = "Run";
                    }
                };
            _cancelableChangedListener = b =>
                {
                    if (b)
                    {
                        RunButton.Content = "Background";
                        RunButton.IsEnabled = true;
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
                ExplanationBlock.Text = explanation.Explanation;
            }
        }

        private void InitializeMonitor()
        {
            _monitor = new WorkMonitor(_work);
            var workText = _monitor.GetElement(this);
            _workText = workText;
            Grid.SetRow(workText, 2);
            Grid.SetColumn(workText, 0);
            Grid.SetColumnSpan(workText, 2);
            WorkControlGrid.Children.Add(workText);
            _monitor.IsRunningChanged.AddListener(_runningChangedListener);
            _monitor.CanPutInBackgroundChanged.AddListener(_cancelableChangedListener);
        }

        private void OnRun(object sender, RoutedEventArgs args)
        {
            if (_monitor.IsRunning && _monitor.CanPutInBackground)
            {
                _monitor.IsRunningChanged.RemoveListener(_runningChangedListener);
                _monitor.CanPutInBackgroundChanged.RemoveListener(_cancelableChangedListener);
                CancelButton.IsEnabled = false;
                RunButton.Content = "Run";
                WorkControlGrid.Children.Remove(_workText);
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
                RunButton.IsEnabled = false;
                CancelButton.IsEnabled = true;
                _monitor.RunWork();                
            }
        }

        private void OnCancel(object sender, RoutedEventArgs args)
        {
            _monitor.IsCanceled = true;
            CancelButton.IsEnabled = false;
        }

        public FrameworkElement GetElement(object container)
        {
            return this;
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
