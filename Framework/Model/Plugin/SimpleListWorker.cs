
using System.Collections.Generic;
using System.ComponentModel.Composition;

using com.antlersoft.HostedTools.Framework.Interface.Menu;
using com.antlersoft.HostedTools.Framework.Interface.UI;
using com.antlersoft.HostedTools.Framework.Model.UI;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    [InheritedExport(typeof(IOutputPaneList))]
    public abstract class SimpleListWorker : SimpleWorker, IOutputPaneList
    {
        public static IList<IOutputPaneSpecifier> ListBoxOutputPanes = (new List<IOutputPaneSpecifier>
            {
                new OutputPaneSpecifier(EOutputPaneType.List)
            }).AsReadOnly();
        protected SimpleListWorker(IEnumerable<IMenuItem> menuEntries, IEnumerable<string> keys)
            : base(menuEntries, keys)
        {
        }

        protected SimpleListWorker(IMenuItem item, IEnumerable<string> keys)
            : base(item, keys)
        {
            
        }

        public virtual EPaneListOrientation Orientation
        {
            get { return EPaneListOrientation.Vertical; }
        }

        public virtual IList<IOutputPaneSpecifier> Panes
        {
            get { return ListBoxOutputPanes; }
        }
    }
}
