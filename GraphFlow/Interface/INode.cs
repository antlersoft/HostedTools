using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.GraphFlow.Interface
{
    public interface INode : IHostedObject, IAsyncBatcher<object>
    {
        Type GetSequenceType(string streamKey);
        Task<IEnumerable<object>> NextBatch(string streamKey);
        void ConnectInput(INode inputNode, string streamKey, string inputKey);
        void DisconnectInput(string inputKey);
        void ConnectOutput(string streamKey);
        void DisconnectOutput(string streamKey);
        bool IsCompletelyConnected {get;}
        bool HasOutputEdges {get;}
    }
}