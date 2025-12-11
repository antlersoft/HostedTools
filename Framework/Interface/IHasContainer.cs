using System;
using System.ComponentModel.Composition.Hosting;

namespace com.antlersoft.HostedTools.Framework.Interface
{
    [Obsolete("This interface is obsolete. Use IAvailableComposer")]
    public interface IHasContainer : IHostedObject
    {
        CompositionContainer Container { get; }
    }
}
