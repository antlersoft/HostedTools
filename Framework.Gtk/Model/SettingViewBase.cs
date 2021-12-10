using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal abstract class SettingViewBase : HostedObjectBase, ISavable, IElementSource
    {
        private bool _needsSave;
        private readonly ListenerCollection<ISavable> _needsSaveChangedListeners = new ListenerCollection<ISavable>();

        internal SettingViewBase()
        {
            _needsSaveChangedListeners = new ListenerCollection<ISavable>();
        }

        internal virtual ISetting Setting { get; set; }

        protected IEnumerable<MenuItem> GetPopupMenuItems()
        {
            yield return new SeparatorMenuItem();

            var importMenuItem = new MenuItem("Import single value from text file");
            importMenuItem.Activated += (object source, EventArgs args) => FileOperation(source as Window, true, false);
            yield return importMenuItem;

            importMenuItem = new MenuItem("Import value history from json");
            importMenuItem.Activated += (object source, EventArgs args) => FileOperation(source as Window, true, true);
            yield return importMenuItem;

            yield return new SeparatorMenuItem();
            importMenuItem = new MenuItem("Export current value to text file");
            importMenuItem.Activated += (object source, EventArgs args1) => FileOperation(source as Window, false, false);
            yield return importMenuItem;

            importMenuItem = new MenuItem("Export values as a json file");
            importMenuItem.Activated += (object source, EventArgs args) => FileOperation(source as Window, false, true);
            yield return importMenuItem;
        }

        void ImportValueFromFile(string path, bool isJson)
        {
            if (isJson)
            {
                string[] values = null;
                using (var rdr = new StreamReader(path))
                {
                    try
                    {
                        values = JsonConvert.DeserializeObject<string[]>(rdr.ReadToEnd());
                    }
                    catch (Exception)
                    {}
                }
                if (values != null)
                {
                    for (int i = values.Length - 1; i>=0; --i)
                    {
                        Setting.SetRaw(values[i]);
                    }
                    return;
                }
            }
            using (var rdr = new StreamReader(path))
            {
                Setting.SetRaw(rdr.ReadToEnd());
            }
        }

        void ExportValueToFile(string path, bool isJson)
        {
            TrySave();
            using (var writer = new StreamWriter(path))
            {
                writer.Write(isJson ? JsonConvert.SerializeObject(Setting.PreviousValues.ToArray())
                    : Setting.GetRaw());
            }
        }

        void FileOperation(Window source, bool isImport, bool isJson)
        {
            object[] param = new object[4];
            param[0] = "Select File";
            param[1] = ResponseType.Accept;
            param[2] = Stock.Cancel;
            param[3] = ResponseType.Cancel;
            FileChooserAction action = isImport ? FileChooserAction.Open : FileChooserAction.Save;
            using (var dlg = new FileChooserDialog($"{(isImport ? "Import" : "Export")} {(isJson ? "json" : "text")} for {(Setting.Definition.Prompt ?? string.Empty)}",
                source, action, param))
            {
                dlg.DefaultResponse = ResponseType.Cancel;
                if (dlg.Run() == (int)ResponseType.Accept)
                {
                    if (isImport)
                    {
                        ImportValueFromFile(dlg.Filename, isJson);
                    }
                    else
                    {
                        ExportValueToFile(dlg.Filename, isJson);
                    }
                }
            }
        }

        public abstract bool TrySave();

        public bool NeedsSave()
        {
            return _needsSave;
        }

        public abstract void Reset();

        public IListenerCollection<ISavable> NeedsSaveChangedListeners { get { return _needsSaveChangedListeners; } }

        public abstract Widget GetElement(object container);

        internal void SetNeedsSave(bool needsSave)
        {
            bool needsNotify = (needsSave != _needsSave);
            _needsSave = needsSave;
            if (needsNotify)
            {
                _needsSaveChangedListeners.NotifyListeners(this);
            }
        }
    }
}
