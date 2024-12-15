using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder {

public interface IGroupExpressionCreator {
    IGroupExpression CreateGroupExpression(IList<IHtExpression> arguments);
}

public interface IFunctionNamespace {
    string Name {get;}
    IDictionary<string,Func<IList<IHtExpression>,IGroupExpression>> AddedGroupFunctions { get; }
    IDictionary<string,Func<IList<IHtValue>, IHtValue>> AddedFunction { get; } 
}

}