using System.Linq;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    class PathView : ComboBoxView
    {
        private Button _browseButton;
        private DockPanel _panel;

        public PathView()
        {
            _browseButton = new Button() {Content = "Browse"};
            _browseButton.Click += Browse;
        }

        public override FrameworkElement GetElement(object container)
        {
            if (_panel == null)
            {
                var combo = base.GetElement(container);
                _panel = new DockPanel();
                DockPanel.SetDock(_browseButton, Dock.Left);
                _panel.Children.Add(_browseButton);
                _panel.Children.Add(combo);
            }
            return _panel;
        }

        private void UpdateText(string text)
        {
            if (_element.Text != text)
            {
                _element.Text = text;
                SetNeedsSave(true);
            }
        }

        private void Browse(object source, RoutedEventArgs args)
        {
            var definition = Setting.Definition.Cast<IPathSettingDefinition>();
            if (definition.IsFolder)
            {
                var dlg = new VistaFolderBrowserDialog()
                    {
                        Description = definition.Description,
                        UseDescriptionForTitle = definition.Description.Length < 30
                    };
                if (dlg.ShowDialog() ?? false)
                {
                    UpdateText(dlg.SelectedPath);
                }
            }
            else if (definition.IsSave)
            {
                var dlg = new SaveFileDialog() { Title = definition.Prompt };
                var ext = definition.FileTypesAndExtensions;
                if (ext != null)
                {
                    dlg.Filter = ext;
                }
                bool? result = dlg.ShowDialog();
                if (result.Value)
                {
                    UpdateText(dlg.FileName);
                }
            }
            else
            {
                var dlg = new OpenFileDialog() { Title = definition.Prompt, CheckFileExists = true};
                var ext = definition.FileTypesAndExtensions;
                if (ext != null)
                {
                    dlg.Filter = ext;
                }
                bool? result = dlg.ShowDialog();
                if (result.Value)
                {
                    UpdateText(dlg.FileName);
                }               
            }
        }
    }
}
