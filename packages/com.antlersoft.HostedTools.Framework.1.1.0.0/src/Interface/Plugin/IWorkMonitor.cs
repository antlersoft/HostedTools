using System;
using System.IO;

namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface IWorkMonitor : IHostedObject
    {
        TextWriter Writer { get; }
        Exception Thrown { get; set; }
    }
}
