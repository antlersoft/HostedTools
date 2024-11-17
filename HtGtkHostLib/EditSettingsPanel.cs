using com.antlersoft.HostedTools.Framework.Gtk.Interface;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using Gtk;
using System.Collections.Generic;
using System.Linq;

namespace com.antlersoft.HostedTools.GtkHostLib
{
    class EditSettingsPanel : HostedObjectBase, ISavable, IElementSource
    {
        private readonly Table _panel;
        private readonly ISettingManager _settings;
        private readonly List<ISavable> _savables;
        private readonly ListenerCollection<ISavable> _listenerCollection;
        private readonly Label _label;
        private readonly uint _definedRows;

        internal EditSettingsPanel(ISettingManager settings, IEnumerable<string> keys)
        {
            var keyList = keys.ToList();
            _settings = settings;
            _savables = new List<ISavable>();
            _panel = new Table((uint)(keyList.Count + 2), 2, false);
            _listenerCollection = new ListenerCollection<ISavable>();
            uint row = 0;
            foreach (var key in keys)
            {
                ISetting setting = settings[key];
                ISettingDefinition def = setting.Definition;
                var creator = def.Cast<IViewCreator>();
                if (creator != null)
                {
                    var view = creator.CreateView();
                    var elemSource = view.Cast<IElementSource>();
                    if (elemSource != null)
                    {
                        var savable = view.Cast<ISavable>();
                        if (savable != null)
                        {
                            _savables.Add(savable);
                        }
                        var label = new Label { Text = string.IsNullOrEmpty(def.Prompt) ? string.Empty : def.Prompt + ":" };
                        label.TooltipText = def.FullKey();
                        _panel.Attach(label, 0, 1, row, row+1, 0, AttachOptions.Fill, 2, 2);
                        var elem = elemSource.GetElement(_panel);
                        _panel.Attach(elem, 1, 2, row, row+1, AttachOptions.Fill|AttachOptions.Expand, AttachOptions.Fill, 2, 2);
                    }
                }
                row++;
            }
            var button = new Button { Label = "Save" };
            _panel.Attach(button, 0, 1, row, row+1, 0, 0, 2, 2);
            button.Clicked += (sender, args) => TrySave();
            _definedRows = row + 1;
            _label = new Label();
            _panel.Attach(_label, 0, 2, _definedRows, _definedRows + 1, AttachOptions.Expand, AttachOptions.Fill, 2, 2);
        }

        public void AddExplanation(IHostedObject hasExplanation)
        {
            IExplanation explanation = hasExplanation.Cast<IExplanation>();
            if (explanation != null)
            {
                _label.Text = explanation.Explanation;
            }
        }

        public bool NeedsSave()
        {
            return _savables.Any(s => s.NeedsSave());
        }

        public bool TrySave()
        {
            foreach (var s in _savables)
            {
                s.TrySave();
            }
            _settings.Save();
            return true;
        }

        public Widget GetElement(object container)
        {
            return _panel;
        }


        public void Reset()
        {
            foreach (var s in _savables)
            {
                s.Reset();
            }
        }

        public IListenerCollection<ISavable> NeedsSaveChangedListeners
        {
            get { return _listenerCollection; }
        }
    }
}
