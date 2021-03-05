using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    [Export(typeof(IPlugin))]
    [Export(typeof(IAfterComposition))]
    public class ViewInjector : HostedObjectBase, IPlugin, IAfterComposition
    {
        class SettingViewCreator<T> : HostedObjectBase, IViewCreator where T : SettingViewBase, new()
        {
            private ISettingManager _manager;
            private ISettingDefinition _definition;

            internal SettingViewCreator(ISettingManager manager, ISettingDefinition definition)
            {
                _manager = manager;
                _definition = definition;
            }

            public IHostedObject CreateView()
            {
                T result = new T { Setting = _manager[_definition.FullKey()] };
                return result;
            }
        }

        class ItemSelectionViewCreator : HostedObjectBase, IViewCreator
        {
            private ISettingManager _manager;
            private ISettingDefinition _definition;
            private INavigationManager _navigation;

            internal ItemSelectionViewCreator(ISettingManager manager, ISettingDefinition definition, INavigationManager navigation)
            {
                _manager = manager;
                _definition = definition;
                _navigation = navigation;
            }

            public IHostedObject CreateView()
            {
                var result = new ItemSelectionView { Setting = _manager[_definition.FullKey()], Navigation = _navigation };
                return result;
            }
        }

        [Import] public ISettingManager SettingManager;
        [Import] public INavigationManager NavigationManager;
        public void AfterComposition()
        {
            foreach (var definition in SettingManager.Scopes.SelectMany(s => s.Settings.Select(d => d.Definition)))
            {
                IAggregator aggregator = definition.Cast<IAggregator>();
                if (aggregator != null)
                {
                    if (definition.Cast<IItemSelectionDefinition>() != null)
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                            new ItemSelectionViewCreator(SettingManager, definition, NavigationManager));
                    }
                    else if (definition.Type == typeof(bool))
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator), new SettingViewCreator<CheckBoxView>(SettingManager, definition));
                    }
                    else if (definition.Cast<ISettingSecurity>() != null &&
                             definition.Cast<ISettingSecurity>().IsPassword)
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                                                        new SettingViewCreator<PasswordView>(SettingManager, definition));
                    }
                    else if (definition.Cast<IPathSettingDefinition>() != null)
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                                                        new SettingViewCreator<PathView>(SettingManager, definition));
                    }
                    else if (definition.Cast<IButtonArray>() != null)
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                            new SettingViewCreator<ButtonsView>(SettingManager, definition));
                    }
                    else if (definition.Cast<IMultiLineValue>() != null &&
                             definition.Cast<IMultiLineValue>().DesiredVisibleLinesOfText > 1)
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                            new SettingViewCreator<MultiLineView>(SettingManager, definition));
                    }
                    else if (typeof(Enum).IsAssignableFrom(definition.Type))
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                                                        new SettingViewCreator<EnumView>(SettingManager, definition));
                    }
                    else if (definition.NumberOfPreviousValues == 0)
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                                                        new SettingViewCreator<TextEntryView>(SettingManager, definition));
                    }
                    else
                    {
                        aggregator.InjectImplementation(typeof(IViewCreator),
                                                        new SettingViewCreator<ComboBoxView>(SettingManager, definition));
                    }
                }
            }
        }

        public string Name
        {
            get { return GetType().FullName; }
        }
    }
}
