using System.ComponentModel.Composition.Hosting;

namespace com.antlersoft.HostedTools.Framework.Interface
{
    public interface IHasContainer : IHostedObject
    {
        CompositionContainer Container { get; }
    }
}
