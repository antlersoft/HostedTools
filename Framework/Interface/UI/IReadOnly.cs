namespace com.antlersoft.HostedTools.Framework.Interface.UI {
    public interface IReadOnly : IHostedObject {
        bool IsReadOnly(string key=null);
        IListenerCollection<IReadOnly> ReadOnlyChangeListeners { get; }
    }
}