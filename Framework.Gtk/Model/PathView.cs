using System;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using Gtk;

namespace com.antlersoft.HostedTools.Framework.Gtk.Model
{
    class PathView : ComboBoxView
    {
        private Button _browseButton;
        private HBox _panel;
        private Window _parent;

        public PathView()
        {
            _browseButton = new Button() { Label = "Browse" };
            _browseButton.Clicked += Browse;
        }

        public override Widget GetElement(object container)
        {
            base.GetElement(container);
            if (container is Window window)
            {
                _parent = window;
            }
            if (_panel == null)
            {
                var combo = _element;
                _panel = new HBox();
                _panel.PackStart(_browseButton, false, true, 0);
                _panel.PackEnd(combo, true, true, 0);
            }
            return _panel;
        }

        private void UpdateText(string text)
        {
            if (_element.ActiveText != text)
            {
                ((ListStore)_element.Model).InsertWithValues(0, text);
                _element.Active = 0;
                SetNeedsSave(true);
            }
        }

        private void Browse(object source, EventArgs args)
        {
            var definition = Setting.Definition.Cast<IPathSettingDefinition>();
            object[] param = new object[4];
            param[1] = ResponseType.Accept;
            param[2] = Stock.Cancel;
            param[3] = ResponseType.Cancel;
            FileFilter filter = null;
            FileChooserAction action = FileChooserAction.Open;
            if (definition.FileTypesAndExtensions != null)
            {  
                filter = new FileFilter();
                int splitCount = 0;
                foreach (var substr in definition.FileTypesAndExtensions.Split('|'))
                {
                    if (splitCount % 2 == 1)
                    {
                        foreach (var pattern in substr.Split(';'))
                        {
                            filter.AddPattern(pattern);
                        }
                    }
                    splitCount ++;
                }
            }
            if (definition.IsFolder)
            {
                param[0] = "Select Folder";
                if (definition.IsSave)
                {
                    action = FileChooserAction.CreateFolder;
                }
                else
                {
                    action = FileChooserAction.SelectFolder;
                }
            }
            else if (definition.IsSave)
            {
                action = FileChooserAction.Save;
                param[0] = "Select Destination";
            }
            else
            {
                param[0] = "Select File";
            }
            using (var dlg = new FileChooserDialog(definition.Description ?? definition.Prompt ?? string.Empty,
                _parent, action, param))
            {
                if (filter != null)
                {
                    dlg.Filter = filter;
                }
                dlg.DefaultResponse = ResponseType.Cancel;
                if (dlg.Run() == (int)ResponseType.Accept)
                {
                    UpdateText(dlg.Filename);
                }
            }
        }
    }
}
