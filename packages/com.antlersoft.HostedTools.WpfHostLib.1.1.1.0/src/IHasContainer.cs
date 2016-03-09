
using System.ComponentModel.Composition.Hosting;


namespace com.antlersoft.HostedTools.WpfHostLib
{
    public interface IHasContainer
    {
        CompositionContainer Container { get; }
    }
}
