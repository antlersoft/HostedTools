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

namespace com.antlersoft.HostedTools.WpfHostLib
{
    /// <summary>
    /// Interaction logic for ComboPanel.xaml
    /// </summary>
    public partial class ComboPanel : UserControl
    {
        public ComboPanel(FrameworkElement top, FrameworkElement bottom)
        {
            InitializeComponent();
            Grid.SetRow(top, 0);
            Grid.SetRow(bottom, 1);
            ComboGrid.Children.Add(top);
            ComboGrid.Children.Add(bottom);
        }
    }
}
