
using System.ComponentModel.Composition.Hosting;


namespace com.antlersoft.HostedTools.WpfHostLib
{
    [Obsolete("This interface is obsolete. Use IAvailableComposer in Framework")]
    public interface IHasContainer
    {
        CompositionContainer Container { get; }
    }
}
