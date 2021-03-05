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
                SetNeedsSave(true);
            }
        }

        private void Browse(object source, EventArgs args)
        {
            var definition = Setting.Definition.Cast<IPathSettingDefinition>();
            if (definition.IsFolder)
            {
                var dlg = new FileChooserDialog(definition.Description ?? definition.Prompt ?? string.Empty,
                    _parent, FileChooserAction.CreateFolder);
                dlg.Modal = true;
                dlg.AddButton("Select Folder", ResponseType.Accept);
                dlg.AddButton("Cancel", ResponseType.Cancel);
                dlg.Show();
                if (dlg.Filename != null)
                {
                    UpdateText(dlg.Filename);
                }
            }
            else if (definition.IsSave)
            {
                var dlg = new FileChooserDialog(definition.Description ?? definition.Prompt ?? string.Empty,
                    _parent, FileChooserAction.Save);
                dlg.Modal = true;
                dlg.AddButton("Select Destination", ResponseType.Accept);
                dlg.AddButton("Cancel", ResponseType.Cancel);
                dlg.Show();
                if (dlg.Filename != null)
                {
                    UpdateText(dlg.Filename);
                }
            }
            else
            {
                var dlg = new FileChooserDialog(definition.Description ?? definition.Prompt ?? string.Empty,
                                    _parent, FileChooserAction.Open);
                dlg.Modal = true;
                dlg.AddButton("Select File", ResponseType.Accept);
                dlg.AddButton("Cancel", ResponseType.Cancel);
                dlg.Show();
                if (dlg.Filename != null)
                {
                    UpdateText(dlg.Filename);
                }
            }
        }
    }
}
