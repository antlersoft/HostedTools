using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Wpf.Interface;

namespace com.antlersoft.HostedTools.WpfHostLib
{
    class EditSettingsPanel : HostedObjectBase, ISavable, IElementSource
    {
        private readonly Grid _panel;
        private readonly ISettingManager _settings;
        private readonly List<ISavable> _savables;
        private readonly ListenerCollection<ISavable> _listenerCollection; 

        internal EditSettingsPanel(ISettingManager settings, IEnumerable<string> keys)
        {
            _settings = settings;
            _savables = new List<ISavable>();
            _panel = new Grid();
            _panel.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            _panel.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
            _listenerCollection = new ListenerCollection<ISavable>();
            int row = 0;
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
                        _panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                        var label = new Label { Content = def.Prompt + ":" };
                        label.ToolTip = def.FullKey();
                        Grid.SetRow(label, row);
                        Grid.SetColumn(label, 0);
                        _panel.Children.Add(label);
                        var elem = elemSource.GetElement(_panel);
                        Grid.SetRow(elem, row);
                        Grid.SetColumn(elem, 1);
                        _panel.Children.Add(elem);
                    }
                }
                row++;
            }
            _panel.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
            var button = new Button { Content = "Save" };
            Grid.SetRow(button, row);
            Grid.SetColumn(button, 0);
            button.Click += (sender, args) => TrySave();
            _panel.Children.Add(button);
        }

        public void AddExplanation(IHostedObject hasExplanation)
        {
            IExplanation explanation = hasExplanation.Cast<IExplanation>();
            if (explanation != null)
            {
                _panel.RowDefinitions.Add(new RowDefinition {Height = GridLength.Auto});
                var label = new Label {Content = explanation.Explanation};
                Grid.SetRow(label, _panel.RowDefinitions.Count - 1);
                Grid.SetColumnSpan(label, 2);
                Grid.SetColumn(label, 0);
                _panel.Children.Add(label);
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

        public FrameworkElement GetElement(object container)
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
