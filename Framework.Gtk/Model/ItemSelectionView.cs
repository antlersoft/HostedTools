using com.antlersoft.HostedTools.Framework.Interface.Navigation;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using Gtk;
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    internal class ItemSelectionView : SettingViewBase
    {
        private readonly ComboBoxText _combobox;
        private readonly ObservableCollection<object> _collection;
        internal INavigationManager Navigation;

        public ItemSelectionView()
        {
            _combobox = new ComboBoxText();
            _collection = new ObservableCollection<object>();
            _combobox.ButtonSensitivity = SensitivityType.Off;
            _combobox.Sensitive = false;
            _combobox.EditingDone += delegate (object sender, EventArgs args)
            {
                if (_combobox.ActiveText!=null)
                {
                    SetNeedsSave(_combobox.ActiveText != Setting.GetRaw());
                }
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
                Setting.SetRaw(ItemSelection.GetRawTextForItem(_collection[_combobox.Active]));
                SetNeedsSave(false);
            }
            return true;
        }

        int ItemIndex(object toMatch)
        {
            int index = 0;
            foreach (var item in ItemSelection.GetAllItems())
            {
                if (item == toMatch)
                {
                    return index;
                }
                index++;
            }
            return 0;
        }

        public override void Reset()
        {
            _collection.Clear();
            _combobox.Model = new ListStore(typeof(string));
            foreach (var val in ItemSelection.GetAllItems())
            {
                _collection.Add(val);
                _combobox.AppendText(ItemSelection.GetRawTextForItem(val));
            }
            object selectedItem = ItemSelection.FindMatchingItem(Setting.GetExpanded());
            if (selectedItem == null)
            {
                selectedItem = _combobox.ActiveText;
                if (selectedItem != null)
                {
                    Setting.SetRaw(ItemSelection.GetRawTextForItem(selectedItem));
                }
            }
            _combobox.Active = ItemIndex(selectedItem);
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
