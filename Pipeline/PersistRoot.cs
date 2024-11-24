using System.ComponentModel.Composition;

namespace com.antlersoft.HostedTools.Pipeline {
    [Export(typeof(IRootNode))]
    public class PersistRoot : AbstractNodePersistence<IRootNode>, IRootNode, IHtValueRoot
    {
        protected override string NodeName => "Root";

        protected override string MenuPrefixKey => "Input";

        public IHtValueSource GetHtValueSource(PluginState state)
        {
            return PluginManager[state.PluginName].Cast<IHtValueRoot>().GetHtValueSource(state);
        }

    }
}