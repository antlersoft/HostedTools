using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Python.Runtime;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
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
	[Export(typeof(IWork))]
	[Export(typeof(IPlugin))]
	public class PythonPlugins : HostedObjectBase, IPluginSource, ISettingDefinitionSource, IMenuItemSource, IWork, IPlugin
    {
        [Import] public IAppConfig Config;
        [Import] public ISettingManager SettingManager;
        [ImportMany] public IEnumerable<IPythonPlugin> PythonTools; 
        private bool _initialized;
        private List<IPlugin> _plugins = new List<IPlugin>();
        private List<ISettingDefinition> _definitions = new List<ISettingDefinition>();
        private List<IMenuItem> _menuItems = new List<IMenuItem>();
		private IMenuItem[] _masterItem = new [] {new MenuItem("Common.ReloadPythonTools", "Reload Python Tools", typeof(PythonPlugins).FullName, "Common")};

		private static readonly string BSymbol = "{QQ}";
		private string cleanupString(string toClean)
		{
			if (toClean.Contains (BSymbol))
			{
				throw new ArgumentException ("Python parameter contains suspicious string");
			}
			return toClean.Replace ("\\", BSymbol).Replace ("'", "\\'").Replace ("\"", "\\\"").Replace (BSymbol, "\\");
		}

        private void Initialize()
        {
            if (!_initialized)
            {
				PyObject coll = StartEngineAndGetToolCollection ();
				if (coll == null)
				{
					return;
				}
				Dictionary<string,IPythonPlugin> toolsByName = PythonTools.ToDictionary(pt => pt.Name);
				using (var py = new PyLock())
				{
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
				return _masterItem.Concat(_menuItems);
            }
        }

		public string Name {
			get { return typeof(PythonPlugins).FullName; }
		}

		public void Perform(IWorkMonitor monitor)
		{
			Initialize ();
			PythonEngine.Shutdown ();
			PyObject coll = StartEngineAndGetToolCollection ();
			if (coll == null)
			{
				return;
			}
			using (new PyLock ())
			{
				int l_coll = coll.Length();
				for (int i = 0; i < l_coll; i++)
				{
					dynamic ht = coll.GetItem(i);
					string name = (string)ht._name;
					IPythonPlugin pp = PythonTools.FirstOrDefault (pt => pt.Name == name);
					if (pp != null)
					{
						pp.SetPythonImplementation (ht);
					}
					pp = _plugins.FirstOrDefault (p => p.Name == name) as IPythonPlugin;
					if (pp != null)
					{
						pp.SetPythonImplementation (ht);
					}
				}
			}
		}

		private PyObject StartEngineAndGetToolCollection()
		{
			string packageToLoad = Config.Get<string>("com.antlersoft.HostedTools.PythonIntegration.PackageName");
			if (packageToLoad == null)
			{
				return null;
			}
			string pythonHome = Config.Get<string> ("com.antlersoft.HostedTools.PythonIntegration.PythonHome");
			if (pythonHome != null)
			{
				PythonEngine.PythonHome = pythonHome;
			}
			string programName = Config.Get<string> ("com.antlersoft.HostedTools.PythonIntegration.ProgramName");
			if (programName != null)
			{
				PythonEngine.ProgramName = programName;
			}
			string pythonPath = Config.Get<string> ("com.antlersoft.HostedTools.PythonIntegration.PythonPath");
			PythonEngine.Initialize();
			PythonEngine.BeginAllowThreads();
			using (var py = new PyLock ())
			{
				if (pythonPath != null)
				{
					foreach (string s in pythonPath.Split(';').Reverse())
					{
						if (-1 == PythonEngine.RunSimpleString ("import sys\r\nsys.path.insert(1,\"" + cleanupString (s) + "\")")) {
							throw new Exception ("Python error adding to path");
						}
					}
				}
				dynamic m1 = PythonEngine.ImportModule ("HostedTools");
				if (m1 == null)
				{
					throw new PythonException ();
				}
				m1._mainEval ("from " + cleanupString (packageToLoad) + " import *");
				PyObject coll = m1._ObjectCollection;
				return coll;
			}
		}
    }
}
