using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Pipeline.Branch {
    public interface IBranchManager : IHostedObject {
        IBranchCollection CreateBranchCollection(IWorkMonitor monitor, string key);
        IBranchCollection RetrieveBranchCollection(IWorkMonitor monitor, string key);
        void FinishBranchCollection(IWorkMonitor monitor, string key);
    }
}