using System;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch {

    public interface IBranchCollection : IHostedObject {
        string Key { get; }
        int Count {get;}
        bool IsFinished { get; }
        IBranchHtValueReceiver GetNextReceiver();
        IHtValueSource GetHtValueSource(int index);
    }
}