using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.Windows;
using com.antlersoft.HostedTools.WpfHostLib;
namespace com.antlersoft.HostedTools.WpfHostTemplate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, IHasContainer
    {
        private CompositionContainer _container;

        public CompositionContainer Container { get { return _container; } }
        public App()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();
            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new ApplicationCatalog());

            //Create the CompositionContainer with the parts in the catalog
            _container = new CompositionContainer(catalog);

            // Set the default connection limit high
            ServicePointManager.DefaultConnectionLimit = 1000;
        }
    }
}
