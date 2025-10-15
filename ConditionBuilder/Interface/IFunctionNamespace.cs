using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface {


    public interface IFunctionNamespace : IHostedObject {
        string Name {get;}
        IDictionary<string,IAddedGroupFunction> AddedGroupFunctions { get; }
        IDictionary<string,IAddedFunction> AddedFunctions { get; } 
    }

}