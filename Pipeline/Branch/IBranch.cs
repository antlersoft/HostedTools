using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch {

    public interface IBranch : IHostedObject {
        string BranchKey {get;}
        int Index { get; }
    }
}