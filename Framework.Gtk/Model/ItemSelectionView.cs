using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using Gtk;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class ItemSelectionView : SettingViewBase
    {
        private readonly ComboBoxText _combobox;
        private readonly ObservableCollection<object> _collection;
        internal INavigationManager Navigation;

        internal override ISetting Setting {
            set {
                base.Setting = value;

                if (value.Definition.Cast<IReadOnly>() is IReadOnly ro) {
                    _combobox.Sensitive = ! ro.IsReadOnly();
                    ro.ReadOnlyChangeListeners.AddListener((a) => {
                        _combobox.Sensitive = ! ro.IsReadOnly();
                    });
                }
            }
        }

        public ItemSelectionView()
        {
            _combobox = new ComboBoxText();
            _collection = new ObservableCollection<object>();
            //_combobox.ButtonSensitivity = SensitivityType.Off;
            //_combobox.Sensitive = false;
            _combobox.Changed += delegate (object sender, EventArgs args)
            {
                if (_combobox.ActiveText!=null)
                {
                    SetNeedsSave(_combobox.ActiveText != Setting.GetRaw());
                }
            };
            _combobox.Mapped += delegate (object sender, EventArgs args)
            {
                Reset();
            };
        }

        private IItemSelectionDefinition ItemSelection
        {
            get { return Setting.Definition.Cast<IItemSelectionDefinition>(); }
        }

        public override bool TrySave()
        {
            var data = _combobox.ActiveText;
            if (data != null)
            {
                var raw = ItemSelection.GetRawTextForItem(_collection[_combobox.Active]);
                if (Setting.GetRaw() != raw) {
                    Setting.SetRaw(ItemSelection.GetRawTextForItem(_collection[_combobox.Active]));
                }
                SetNeedsSave(false);
            }
            return true;
        }

        int ItemIndex(object toMatch)
        {
            int index = 0;
            foreach (var item in ItemSelection.GetAllItems())
            {
                if (item.Equals(toMatch))
                {
                    return index;
                }
                index++;
            }
            return 0;
        }

        string ItemText(object item)
        {
            if (item is ItemSelectionItem sel)
            {
                return sel.ItemDescription;
            }
            return item.ToString();
        }

        public override void Reset()
        {
            _collection.Clear();
            _combobox.Model = new ListStore(typeof(string));
            foreach (var val in ItemSelection.GetAllItems())
            {
                _collection.Add(val);
                _combobox.AppendText(ItemText(val));
            }
            object selectedItem = ItemSelection.FindMatchingItem(Setting.GetExpanded());
            if (selectedItem == null)
            {
                _combobox.Active = 0;
            }
            else
            {
                _combobox.Active = ItemIndex(selectedItem);
            }
            SetNeedsSave(false);
        }

        private void Edit(object source, EventArgs args)
        {
            Navigation.NavigateTo(ItemSelection.NavigateToOnEdit(_collection[_combobox.Active]));
        }

        public override Widget GetElement(object container)
        {
            Setting.SettingChangedListeners.AddListener(s =>
            { Application.Invoke(delegate { Reset(); }); });
            _combobox.TooltipText = Setting.Definition.Description;
            Reset();
            if (!ItemSelection.IncludeEditButton())
            {
                return _combobox;
            }
            var panel = new HBox();
            var editButton = new Button { Label = "Edit" };
            editButton.Clicked += Edit;
            panel.PackStart(editButton, false, false, 0);
            panel.PackEnd(_combobox, true, true, 0);
            return panel;
        }
    }
}
