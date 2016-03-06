using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.UI;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using Newtonsoft.Json;
using Python.Runtime;

namespace com.antlersoft.HostedTools.PythonIntegration
{
    public class PythonTool : HostedObjectBase, IPythonPlugin, ISettingEditList, IWork
    {
        private string _name;
        private ISettingManager _settingManager;
        private PyObject _pythonImplementation;
        private List<string> _keysToEdit;
        private JsonSerializer _serializer;
        private JsonSerializerSettings _settings;

        private static readonly OutputPaneList PaneList = new OutputPaneList(EPaneListOrientation.Vertical,
                                                                              new[]
                                                                                  {
                                                                                      new OutputPaneSpecifier(
                                                                                  EOutputPaneType.Text, null, 1),
                                                                                      new OutputPaneSpecifier(
                                                                                  EOutputPaneType.Grid, null, 3)
                                                                                  });

        internal PythonTool(string name, PyObject pythonImplementation, ISettingManager settingManager, List<string> keysToEdit)
        {
            var jsonFactory = new JsonFactory();
            _serializer = jsonFactory.GetSerializer(true);
            _settings = jsonFactory.GetSettings(true);
            _name = name;
            _settingManager = settingManager;
            if (pythonImplementation.GetAttr("_isGrid").IsTrue())
            {
                InjectImplementation(typeof (IOutputPaneList), PaneList);
            }
            SetPythonImplementation(pythonImplementation);
            _keysToEdit = keysToEdit;
        }
        public void Perform(IWorkMonitor monitor)
        {
            using (new PyLock())
            {
                _pythonImplementation.InvokeMethod("Perform", PyObject.FromManagedObject(monitor));
            }
        }

        public void SetPythonImplementation(PyObject pythonImplementation)
        {
            _pythonImplementation = pythonImplementation;
            _pythonImplementation.InvokeMethod("SetHostedTool", PyObject.FromManagedObject(this));
        }

        public virtual void WriteRow(IWorkMonitor monitor, string serializedObject)
        {
            WriteRecord(monitor, JsonConvert.DeserializeObject<IHtValue>(serializedObject, _settings));
        }

        public string Value(string id)
        {
            return _settingManager[id].GetExpanded();
        }

        public void SetValue(string id, string val)
        {
            _settingManager[id].SetRaw(val);
        }

        public string SerializeHtValue(IHtValue val)
        {
            return JsonConvert.SerializeObject(val, _settings);
        }

        public string Name { get { return _name; } }

        public IEnumerable<string> KeysToEdit
        {
            get { return _keysToEdit; }
        }

        private static object ValueToSimpleObject(JsonSerializer serializer, IHtValue val)
        {
            object result;
            if (val == null)
            {
                result = string.Empty;
            }
            else if (val.IsBool)
            {
                result = val.AsBool;
            }
            else if (val.IsDouble)
            {
                result = val.AsDouble;
            }
            else if (val.IsEmpty)
            {
                result = string.Empty;
            }
            else if (val.IsString)
            {
                result = val.AsString;
            }
            else
            {
                using (var writer = new StringWriter())
                {
                    serializer.Serialize(writer, val);
                    result = writer.ToString();
                }
            }
            return result;
        }

        private void WriteRecord(IGridOutput grid, IHtValue record)
        {
            var kvpList = record.IsDictionary ? record.AsDictionaryElements.OrderBy(k => k.Key).ToList() : new List<KeyValuePair<string, IHtValue>>() { new KeyValuePair<string, IHtValue>("{no columns}", record) };
            Dictionary<string, object> row = new Dictionary<string, object>(kvpList.Count);
            foreach (var kvp in kvpList)
            {
                row.Add(kvp.Key, ValueToSimpleObject(_serializer, kvp.Value));
            }
            grid.AddRow(row);
        }

        private void WriteRecord(IWorkMonitor monitor, IHtValue val)
        {
            var hasOutput = monitor.Cast<IHasOutputPanes>();
            IGridOutput grid = null;
            if (hasOutput != null)
            {
                grid = hasOutput.FindGridOutput();
            }
            if (grid == null)
            {
                monitor.Writer.WriteLine(String.Join(",",
                    val.AsDictionaryElements.Select(kvp => ValueToSimpleObject(_serializer, kvp.Value).ToString())));
            }
            else
            {
                WriteRecord(grid, val);
            }
        }

    }
}
