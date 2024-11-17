using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.Pipeline {
    [Export(typeof(IStemNode))]
    public class PersistStem : AbstractNodePersistence<IStemNode>, IStemNode, IHtValueStem
    {
        protected override string NodeName => "Stem";

        protected override string MenuPrefixKey => "Transform";

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return PluginManager[state.PluginName].Cast<IHtValueStem>().GetHtValueTransform(state);
        }

    }
}