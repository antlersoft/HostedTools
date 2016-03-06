using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using Python.Runtime;

namespace com.antlersoft.HostedTools.PythonIntegration
{
    class PythonTool : HostedObjectBase, IPlugin, ISettingEditList, IWork
    {
        private string _name;
        private ISettingManager _settingManager;
        private PyObject _pythonImplementation;
        private List<string> _keysToEdit; 

        internal PythonTool(string name, PyObject pythonImplementation, ISettingManager settingManager, List<string> keysToEdit)
        {
            _name = name;
            _settingManager = settingManager;
            _pythonImplementation = pythonImplementation;
            _pythonImplementation.InvokeMethod("SetHostedTool", PyObject.FromManagedObject(this));
            _keysToEdit = keysToEdit;
        }
        public void Perform(IWorkMonitor monitor)
        {
            using (var py = Py.GIL())
            {
                _pythonImplementation.InvokeMethod("Perform", PyObject.FromManagedObject(monitor));
            }
        }

        public string Value(string id)
        {
            return _settingManager[id].GetExpanded();
        }

        public string Name { get { return _name; } }

        public IEnumerable<string> KeysToEdit
        {
            get { return _keysToEdit; }
        }
    }
}
