using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Python.Runtime;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.PythonIntegration
{
    class PyLock : IDisposable
    {
        IntPtr _gs;

        internal PyLock()
        {
            _gs = PythonEngine.AcquireLock();
        }

        public void Dispose()
        {
            PythonEngine.ReleaseLock(_gs);
        }
    }
    [Export(typeof(IPluginSource))]
    [Export(typeof(ISettingDefinitionSource))]
    [Export(typeof(IMenuItemSource))]
    public class PythonPlugins : HostedObjectBase, IPluginSource, ISettingDefinitionSource, IMenuItemSource
    {
        [Import] public IAppConfig Config;
        [Import] public ISettingManager SettingManager;
        [ImportMany] public IEnumerable<IPythonPlugin> PythonTools; 
        private bool _initialized;
        private List<IPlugin> _plugins = new List<IPlugin>();
        private List<ISettingDefinition> _definitions = new List<ISettingDefinition>();
        private List<IMenuItem> _menuItems = new List<IMenuItem>();

        private void Initialize()
        {
            if (!_initialized)
            {
                string packageToLoad = Config.Get<string>("com.antlersoft.HostedTools.PythonIntegration.PackageName");
                Dictionary<string,IPythonPlugin> toolsByName = PythonTools.ToDictionary(pt => pt.Name);
                if (packageToLoad == null)
                {
                    return;
                }
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
                using (var py = new PyLock())
                {
                    PythonEngine.RunSimpleString("from " + packageToLoad + " import *");
                    dynamic m1 = PythonEngine.ImportModule("HostedTools");
                    //string s = PythonEngine.Platform + " " + PythonEngine.PythonHome;
                    //dynamic module = PythonEngine.ImportModule("HostedTools");
                    PyObject coll = m1._ObjectCollection;
                    int l_coll = coll.Length();
                    for (int i=0; i<l_coll; i++)
                    {
                        dynamic ht = coll.GetItem(i);
                        PyObject menus = ht._menuItems;
                        int l_menus = menus.Length();
                        for (int j=0; j<l_menus; j++)
                        {
                            _menuItems.Add((IMenuItem)menus.GetItem(j).AsManagedObject(typeof(IMenuItem)));
                        }
                        PyObject definitions = ht._settingDefinitions;
                        int l_definitions = definitions.Length();
                        for (int j = 0; j < l_definitions; j++)
                        {
                            _definitions.Add((ISettingDefinition)definitions.GetItem(j).AsManagedObject(typeof(ISettingDefinition)));
                        }
                        List<string> settingNameList = new List<string>();
                        PyObject nameList = ht._settingNames;
                        int l_nameList = nameList.Length();
                        for (int j = 0; j < l_nameList; j++)
                        {
                            settingNameList.Add(nameList.GetItem(j).ToString());
                        }
                        string pluginName = (string) ht._customToolName;
                        IPythonPlugin pp;
                        if (pluginName != null && toolsByName.TryGetValue(pluginName, out pp))
                        {
                            pp.SetPythonImplementation(ht);
                        }
                        else
                        {
                            _plugins.Add(new PythonTool((string)ht._name, (PyObject)ht, SettingManager, settingNameList));
                        }
                    }
                }
                _initialized = true;
            }
        }

        public IEnumerable<IPlugin> SourcePlugins { get { Initialize(); return _plugins; } }
        public IEnumerable<ISettingDefinition> Definitions { get { Initialize(); return _definitions; } }

        public IEnumerable<IMenuItem> Items
        {
            get
            {
                Initialize();
                return _menuItems;
            }
        }
    }
}
