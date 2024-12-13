using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch {

public interface IBranchHtValueReceiver : IBranch {
    void ReceiveRow(IHtValue row);
    void Finish();
}

}