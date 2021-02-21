using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Text;
using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
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
        public INavigationManager NavigationManager { get; set; }

        private Exception _startupError;

        private MenuBar _mainMenuBar;

        private Button _backButton;
        private Button _forwardButton;

        private Dictionary<string, Widget> _targetCache = new Dictionary<string, Widget>();

        private Menu _fileMenu;

        private Widget _currentPanel;

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
            hbox.PackStart(_mainMenuBar, true, true, 0);
            _backButton = new Button("<");
            _forwardButton = new Button(">");
            HBox buttonBox = new HBox(true, 2);

            buttonBox.PackStart(_backButton, false, false, 0);
            buttonBox.PackEnd(_forwardButton, false, false, 0);

            hbox.PackEnd(buttonBox, false, false, 0);
            var vbox = new VBox(false, 2);
            vbox.PackStart(hbox, false, false, 0);
            Add(vbox);
        }

        protected override void OnShowAll()
        {
            base.OnShowAll();
            if (_startupError != null)
            {
                Console.WriteLine(_startupError.ToString());
                Close();
            }
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }
    }
}
