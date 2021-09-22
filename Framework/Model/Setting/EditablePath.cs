using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using System;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Model.Setting
{
    /// <summary>
    /// Inject to a PathSettingDefinition to mark it as an editable path
    /// </summary>
    public class EditablePath : HostedObjectBase, IEditablePath
    {
        bool _isEditable = true;
        public bool PathIsEditable
        {
            get
            {
                return _isEditable;
            }
            set
            {
                if (value != _isEditable)
                {
                    _isEditable = value;
                    EditableChanged.NotifyListeners(this);
                }
            }
        }

        public INotifiableListenerCollection<IEditablePath> EditableChanged { get; } = new ListenerCollection<IEditablePath>();

        public Func<ISetting,Task> EditPath { get; set; }

        public INotifiableListenerCollection<ISetting> StartEditing { get; } = new ListenerCollection<ISetting>();

        public INotifiableListenerCollection<ISetting> FinishedEditing { get; } = new ListenerCollection<ISetting>();
    }
}
