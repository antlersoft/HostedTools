using System;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch {

    public interface IBranchCollection : IHostedObject {
        string Key { get; }
        int Count {get;}
        bool IsFinished { get; }
        IBranchHtValueReceiver GetNextReceiver(Func<IHtValue> producer);
        IHtValueSource GetHtValueSource(int index);
    }
}