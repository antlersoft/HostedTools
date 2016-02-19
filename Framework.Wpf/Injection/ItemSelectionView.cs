using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Setting;

namespace com.antlersoft.HostedTools.Framework.Wpf.Injection
{
    internal class ItemSelectionView : SettingViewBase
    {
        private readonly ComboBox _combobox;
        private readonly ObservableCollection<object> _collection; 
        internal INavigationManager Navigation;

        public ItemSelectionView()
        {
            _combobox = new ComboBox();
            _collection = new ObservableCollection<object>();
            _combobox.IsEditable = false;
            _combobox.IsReadOnly = false;
            _combobox.ItemsSource = _collection;
            _combobox.SelectionChanged += delegate(object sender, SelectionChangedEventArgs args)
                {
                    if (args.AddedItems.Count > 0)
                    {
                        SetNeedsSave(args.AddedItems[0].ToString() != Setting.GetRaw());
                    }
                };
        }

        private IItemSelectionDefinition ItemSelection
        {
            get { return Setting.Definition.Cast<IItemSelectionDefinition>(); }
        }

        public override bool TrySave()
        {
            Setting.SetRaw(ItemSelection.GetRawTextForItem(_combobox.SelectedItem));
            SetNeedsSave(false);
            return true;
        }

        public override void Reset()
        {
            _collection.Clear();
            foreach (var val in ItemSelection.GetAllItems())
            {
                _collection.Add(val);
            }
            object selectedItem = ItemSelection.FindMatchingItem(Setting.GetExpanded());
            if (selectedItem == null)
            {
                selectedItem = _combobox.Items[0];
                Setting.SetRaw(ItemSelection.GetRawTextForItem(selectedItem));
            }
            _combobox.SelectedItem = selectedItem;
            SetNeedsSave(false);
        }

        private void Edit(object source, RoutedEventArgs args)
        {
            Navigation.NavigateTo(ItemSelection.NavigateToOnEdit(_combobox.SelectedItem));
        }

        public override FrameworkElement GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener(s =>
                { _combobox.Dispatcher.BeginInvoke(new Action(Reset)); });
            _combobox.ToolTip = Setting.Definition.Description;
            Reset();
            if (! ItemSelection.IncludeEditButton())
            {
                return _combobox;
            }
            var panel = new DockPanel();
            var editButton = new Button {Content = "Edit"};
            editButton.Click += Edit;
            DockPanel.SetDock(editButton, Dock.Left);
            panel.Children.Add(editButton);
            panel.Children.Add(_combobox);
            return panel;
        }
    }
}
