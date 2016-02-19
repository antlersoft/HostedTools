using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Navigation;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [Import]
        public IMenuManager MenuManager { get; set; }
        [Import]
        public IPluginManager PluginManager { get; set; }
        [ImportMany]
        public IEnumerable<IAfterComposition> NeedAfterComposition { get; set; }
        
        [Import]
        public IBackgroundWorkReceiver BackgroundWorkReceiver { get; set; }

        [Import]
        public ISettingManager SettingManager { get; set; }

        [Import]
        public INavigationManager NavigationManager { get; set; }

        public MainWindow()
        {
            ((IHasContainer)Application.Current).Container.ComposeParts(this);
            foreach (IAfterComposition ac in NeedAfterComposition)
            {
                ac.AfterComposition();
            }
            InitializeComponent();
            NavigationManager.NavigationListeners.AddListener(OnNavigate);
            BackButton.Click += (sender, args) => NavigationManager.GoBack();
            BackButton.IsEnabled = false;
            ForwardButton.Click += (sender, args) => NavigationManager.GoForward();
            ForwardButton.IsEnabled = false;
            BuildMenu();
            MenuManager.AddChangeListener(BuildMenu);
        }

        private Dictionary<string,FrameworkElement> _targetCache = new Dictionary<string, FrameworkElement>();
        private MenuItem _fileMenu;
        private FrameworkElement _currentPanel;

        private void BuildMenu()
        {
            Menu menu = MainMenu;
            menu.Items.Clear();
            menu.Items.Add(GetFileMenu());
            foreach (var item in MenuManager.GetChildren(null).OrderBy(s => s.Prompt))
            {
                menu.Items.Add(CreateMenuItem(item));
            }            
        }

        private MenuItem CreateMenuItem(IMenuItem item)
        {
            var result = new MenuItem();
            result.Header = item.Prompt;
            if (!String.IsNullOrEmpty(item.ActionId))
            {
                result.Click += (sender, args) => OnClick(item);
            }
            foreach (var child in MenuManager.GetChildren(item).OrderBy(s => s.Prompt))
            {
                result.Items.Add(CreateMenuItem(child));
            }
            return result;
        }

        private FrameworkElement CreatePanel(ISettingEditList editList, IWork work, IHasSettingChangeActions settingChangeActions)
        {
            IElementSource editPanel = null;
            IElementSource workPanel = null;
            if (editList != null)
            {
                var keys = editList.KeysToEdit.ToList();
                if (keys.Count > 0)
                {
                    editPanel = new EditSettingsPanel(SettingManager, keys);
                    if (work == null)
                    {
                        ((EditSettingsPanel)editPanel).AddExplanation(editList as IHostedObject);
                    }
                }
            }
            if (settingChangeActions != null)
            {
                foreach (var kvp in settingChangeActions.ActionsBySettingKey)
                {
                    Action<IWorkMonitor,ISetting> isa = kvp.Value;
                    ISavable savable = editPanel as ISavable;
                    SettingManager[kvp.Key].SettingChangedListeners.AddListener(s =>
                    {
                        if (savable != null)
                        {
                            if (! savable.TrySave())
                            {
                                return;
                            }
                        }
                        new SettingUpdateActionMonitor().RunUpdateAction(isa, s);
                    });
                }
            }
            if (work != null)
            {
                workPanel = new WorkControl(BackgroundWorkReceiver, work, editPanel == null ? null : editPanel as ISavable);
            }
            if (editPanel != null && workPanel == null)
            {
                return editPanel.GetElement(this);
            }
            if (editPanel == null)
            {
                if (workPanel != null)
                {
                    return workPanel.GetElement(this);
                }
                return null;
            }
            // Case when both panels are created
            return new ComboPanel(editPanel.GetElement(this), workPanel.GetElement(this));
        }

        private MenuItem GetFileMenu()
        {
            if (_fileMenu == null)
            {
                _fileMenu = new MenuItem { Header = "File" };
                var saveItem = new MenuItem { Header = "Save" };
                saveItem.Click += (sender, args) => SettingManager.Save();
                _fileMenu.Items.Add(saveItem);
                _fileMenu.Items.Add(new Separator());
                var exitItem = new MenuItem { Header = "Exit" };
                exitItem.Click += (sender, args) => Close();
                _fileMenu.Items.Add(exitItem);
            }
            return _fileMenu;
        }

        private void OnClick(IMenuItem item)
        {
            var actionId = item.ActionId;
            NavigationManager.NavigateTo(actionId);
        }

        private void OnNavigate(INavigationManager navigation)
        {
            var actionId = navigation.CurrentLocation;
            FrameworkElement newContent;
            if (!_targetCache.TryGetValue(actionId, out newContent))
            {
                IPlugin plugin = PluginManager[actionId];
                if (plugin == null)
                {
                    MessageBox.Show("Can not find plugin for action " + actionId);
                    throw new RejectNavigationException();
                }
                var elementSource = plugin.Cast<IElementSource>();
                if (elementSource != null)
                {
                    newContent = elementSource.GetElement(this);
                }
                else
                {
                    var keyListSource = plugin.Cast<ISettingEditList>();
                    var worker = plugin.Cast<IWork>();
                    if (keyListSource != null || worker != null)
                    {
                        newContent = CreatePanel(keyListSource, worker, plugin.Cast<IHasSettingChangeActions>());
                    }
                    else
                    {
                        MessageBox.Show("Can't figure out how to display " + actionId);
                        throw new RejectNavigationException();
                    }
                }
                if (newContent == null)
                {
                    MessageBox.Show("Action panel would be empty: " + actionId);
                    throw new RejectNavigationException();
                }
                _targetCache[actionId] = newContent;
            }
            if (_currentPanel != null)
            {
                MainDock.Children.Remove(_currentPanel);
            }
            DockPanel.SetDock(newContent, Dock.Bottom);
            MainDock.Children.Add(newContent);
            ActionTitle.Content = RecursiveFind(null, actionId).GetBreadCrumbString(MenuManager);
            _currentPanel = newContent;
            ForwardButton.IsEnabled = navigation.Forward.Count > 0;
            BackButton.IsEnabled = navigation.History.Count > 0;
        }

        private IMenuItem RecursiveFind(IMenuItem parent, string actionId)
        {
            foreach (var item in MenuManager.GetChildren(parent))
            {
                if (actionId == item.ActionId)
                {
                    return item;
                }
                var child = RecursiveFind(item, actionId);
                if (child != null)
                {
                    return child;
                }
            }
            return null;
        }
    }
}
