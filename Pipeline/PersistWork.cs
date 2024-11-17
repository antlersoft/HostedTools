using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Pipeline {
    class GridWorkerImpl : GridWorker
    {
        internal GridWorkerImpl()
        : base(new IMenuItem[0], new string[0])
        {}
        public override void Perform(IWorkMonitor monitor)
        {
            throw new System.NotImplementedException();
        }
    }
    [Export(typeof(IWork))]
    [Export(typeof(IWorkNode))]
    [Export(typeof(IHasOutputPanes))]
    public class PersistWork : AbstractNodePersistence<IWorkNode>, IWork, IOutputPaneList,
        IPartImportsSatisfiedNotification, IWorkNode
    {
        [Import]
        public IHasContainer HasContainer { get; set; }
        private GridWorker _gridImpl;
        public EPaneListOrientation Orientation => _gridImpl.Orientation;

        public IList<IOutputPaneSpecifier> Panes => _gridImpl.Panes;

        protected override string NodeName => "Tree";

        protected override string MenuPrefixKey => string.Empty;

        public void OnImportsSatisfied()
        {
            _gridImpl = new GridWorkerImpl();
            HasContainer.Container.SatisfyImportsOnce(_gridImpl);
        }

        public void Perform(IWorkMonitor monitor)
        {
            var state = GetPluginState();
            Perform(state, monitor);
        }

        public void Perform(PluginState state, IWorkMonitor monitor)
        {
            var node = PluginManager[state.PluginName]?.Cast<IWorkNode>();
            if (node == null) {
                monitor.Writer.WriteLine("Couldn't find plugin to execute");
                return;
            }
            node.Perform(state, monitor);
        }
    }
}