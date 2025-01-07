using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.Navigation;
using Gtk;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    [Export(typeof(IHasContainer))]
    public class MainWindow : Window, IHasContainer
    {
        public CompositionContainer Container { get; private set; }
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
        public IWorkMonitorHolder MonitorHolder { get; set; }

        [Import]
        public INavigationManager NavigationManager { get; set; }

        private Exception _startupError;

        private MenuBar _mainMenuBar;

        private Button _backButton;
        private Button _forwardButton;

        private Dictionary<string, Widget> _targetCache = new Dictionary<string, Widget>();

        private Menu _fileMenu;

        private VBox _vbox;

        private Widget _currentPanel;
        private Label _breadCrumb;
        private HashSet<ISavable> _currentlySaving=new HashSet<ISavable>();

        private bool _shownOnce = false;

        public MainWindow(string s = null)
        : base(s??"HostedTools Gtk Host")
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new ApplicationCatalog());

            //Create the CompositionContainer with the parts in the catalog
            Container = new CompositionContainer(catalog);

            try
            {
                Container.ComposeParts(this);
                foreach (IAfterComposition ac in NeedAfterComposition)
                {
                    ac.AfterComposition();
                }
            }
            catch (Exception ex)
            {
                _startupError = ex;
            }
            SetDefaultSize(1000, 700);
            SetPosition(WindowPosition.Center);
            DeleteEvent += delegate { Application.Quit(); };

            var hbox = new HBox(false, 2);
            _mainMenuBar = new MenuBar();
            BuildMenu();
            hbox.PackStart(_mainMenuBar, true, true, 0);
            _backButton = new Button("<");
            _forwardButton = new Button(">");
            HBox buttonBox = new HBox(true, 2);

            buttonBox.PackStart(_backButton, false, false, 0);
            buttonBox.PackEnd(_forwardButton, false, false, 0);

            hbox.PackEnd(buttonBox, false, false, 0);
            _vbox = new VBox(false, 2);
            _vbox.PackStart(hbox, false, false, 0);
            _breadCrumb = new Label();
            _vbox.PackStart(_breadCrumb, false, false, 0);
            Add(_vbox);
        }

        protected override void OnShowAll()
        {
            base.OnShowAll();
            if (_startupError != null)
            {
                Console.WriteLine(_startupError.ToString());
                new MessageDialog(this, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, _startupError.ToString());
                Close();
            }
            else
            {
                if (!_shownOnce)
                {
                    _shownOnce = true;
                    NavigationManager.NavigationListeners.AddListener(OnNavigate);
                    _backButton.Clicked += (sender, args) => NavigationManager.GoBack();
                    _backButton.Sensitive = false;
                    _forwardButton.Clicked += (sender, args) => NavigationManager.GoForward();
                    _forwardButton.Sensitive = false;
                    // MenuManager.AddChangeListener(BuildMenu);
                    if (SettingManager["Common.UseStartItem"].Get<bool>())
                    {
                        NavigationManager.NavigateTo(SettingManager["Common.StartItem"].Get<string>());
                    }
                }
            }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        private Menu GetFileMenu()
        {
            if (_fileMenu == null)
            {
                _fileMenu = new Menu();
                _fileMenu.Name = "File";
                MenuItem item = new MenuItem("Save");
                _fileMenu.Add(item);
                item.Activated += (sender, args) => SettingManager.Save();
                _fileMenu.Add(new SeparatorMenuItem());
                var exitItem = new MenuItem("Exit");
                exitItem.Activated += (sender, args) => Close();
                _fileMenu.Add(exitItem);
            }
            return _fileMenu;
        }

        private static void AddSubMenu(MenuShell shell, Menu m)
        {
            MenuItem item = new MenuItem(m.Name);
            item.Submenu = m;
            shell.Add(item);
        }
        private void BuildMenu()
        {
            MenuBar menu = _mainMenuBar;

            AddSubMenu(menu, GetFileMenu());
            foreach (var item in MenuManager.GetChildren(null).OrderBy(s => s.Prompt))
            {
                AddChild(menu, item);
            }
        }

        private void OnClick(IMenuItem item)
        {
            var actionId = item.ActionId;
            NavigationManager.NavigateTo(actionId);
        }

        private void AddChild(MenuShell parent, IMenuItem item)
        {
            MenuItem mi = new MenuItem(item.Prompt);
            if (!string.IsNullOrEmpty(item.ActionId))
            {
                mi.Activated += (sender, args) => OnClick(item);
            }
            Menu submenu = null;
            foreach (var child in MenuManager.GetChildren(item).OrderBy(s => s.Prompt))
            {
                if (submenu == null)
                {
                    submenu = new Menu();
                    submenu.Name = item.Prompt;
                }
                AddChild(submenu, child);
            }
            if (submenu != null)
            {
                mi.Submenu = submenu;
            }
            parent.Add(mi);
        }

        private Widget CreatePanel(ISettingEditList editList, IWork work, IHasSettingChangeActions settingChangeActions)
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
                    Action<IWorkMonitor, ISetting> isa = kvp.Value;
                    ISavable savable = editPanel as ISavable;
                    SettingManager[kvp.Key].SettingChangedListeners.AddListener(s =>
                    {
                        if (savable != null && ! _currentlySaving.Contains(savable))
                        {
                            try {
                                _currentlySaving.Add(savable);
                                if (!savable.TrySave())
                                {
                                    return;
                                }
                            } finally {
                                _currentlySaving.Remove(savable);
                            }
                        }
                        new SettingUpdateActionMonitor(this, MonitorHolder).RunUpdateAction(isa, s);
                    });
                }
            }
            if (work != null)
            {
                workPanel = new WorkControl(BackgroundWorkReceiver, work, editPanel == null ? null : editPanel as ISavable, MonitorHolder);
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
        private void OnNavigate(INavigationManager navigation)
        {
            var actionId = navigation.CurrentLocation;
            Widget newContent;
            if (!_targetCache.TryGetValue(actionId, out newContent))
            {
                IPlugin plugin = PluginManager[actionId];
                if (plugin == null)
                {
                    new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Can not find plugin for action " + actionId);
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
                        new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Can't figure out how to display " + actionId);
                        throw new RejectNavigationException();
                    }
                }
                if (newContent == null)
                {
                    new MessageDialog(this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, "Action panel would be empty: "+actionId);
                    throw new RejectNavigationException();
                }
                _targetCache[actionId] = newContent;
            }
            if (_currentPanel != null)
            {
                _vbox.Remove(_currentPanel);
            }
            _vbox.PackEnd(newContent, true, true, 0);
            _breadCrumb.Text = RecursiveFind(null, actionId).GetBreadCrumbString(MenuManager);
            _currentPanel = newContent;
            ShowAll();
            _forwardButton.Sensitive = navigation.Forward.Count > 0;
            _backButton.Sensitive = navigation.History.Count > 0;
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

