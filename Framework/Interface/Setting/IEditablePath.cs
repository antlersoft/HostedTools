using System;

using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.Setting
{
    public interface IEditablePath : IHostedObject
    {
        bool PathIsEditable { get; }
        INotifiableListenerCollection<IEditablePath> EditableChanged { get; }
        Func<ISetting,Task> EditPath { get; }
        INotifiableListenerCollection<ISetting> StartEditing { get; }
        INotifiableListenerCollection<ISetting> FinishedEditing { get; }
    }
}
