using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface
{
    public interface IHostedObject
    {
        T Cast<T>(bool fromAggregated = false) where T : class;
    }
}
