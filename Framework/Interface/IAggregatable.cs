using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface
{
    public interface IAggregatable : IHostedObject
    {
        void SetAggregator(IHostedObject aggregator);
    }
}
