using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    /// <summary>
    /// Interaction logic for BackgroundItem.xaml
    /// </summary>
    public partial class BackgroundItem : UserControl
    {
        public BackgroundItem(IWorkMonitor monitor, FrameworkElement output)
        {
            InitializeComponent();
            Grid.SetRow(output, 2);
            Grid.SetColumn(output, 0);
            Grid.SetColumnSpan(output, 2);
            BackgroundItemGrid.Children.Add(output);
            OnCloseListener = new ListenerCollection<bool>();
            CloseButton.Click += (sender, args) => ((ListenerCollection<bool>) OnCloseListener).NotifyListeners(true);
            WorkMonitor wm = monitor.Cast<WorkMonitor>();
            if (wm != null)
            {
                wm.IsRunningChanged.AddListener(b => CancelButton.IsEnabled = b);
                CancelButton.IsEnabled = wm.IsRunning;
                CancelButton.Click += (sender, args) => { wm.IsCanceled = true; };
            }
        }

        public IListenerCollection<bool> OnCloseListener { get; private set; } 
    }
}
