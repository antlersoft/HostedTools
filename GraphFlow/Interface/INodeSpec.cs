using System.Collections.Generic;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.GraphFlow.Interface
{
    public interface INodeSpec : IHostedObject
    {
        IHtValue InputSchema {get;}
        IHtValue OutputSchema {get;}

        INode GetNode(Dictionary<string,string> settings);
    }
}