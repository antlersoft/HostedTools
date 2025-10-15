using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface
{
    public interface IAddedFunction : IHostedObject
    {
        string Name { get; }
        Func<IList<IHtValue>, IHtValue> Functor { get; }
    }

}