
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;
using Python.Runtime;

namespace com.antlersoft.HostedTools.PythonIntegration
{
    public interface IPythonPlugin : IPlugin
    {
        void WriteRow(IWorkMonitor monitor, string serializedObject);
        string Value(string id);
        void SetValue(string id, string val);
        void SetPythonImplementation(PyObject pythonImplementation);
        string SerializeHtValue(IHtValue val);
    }
}
