using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.Pipeline {
    [Export(typeof(IStemNode))]
    public class PersistStem : AbstractNodePersistence<IStemNode>, IStemNode
    {
        protected override string NodeName => "Stem";

        protected override string MenuPrefixKey => "Transform";
    }
}