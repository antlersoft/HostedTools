using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface
{
    public interface IAddedGroupFunction : IHostedObject
    {
        string Name { get; }
        Func<IList<IHtExpression>,IHtExpression> Functor { get; }
    }
}