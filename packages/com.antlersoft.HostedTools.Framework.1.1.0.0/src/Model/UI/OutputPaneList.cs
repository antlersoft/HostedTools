using System.Collections.Generic;
using System.Linq;

using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Model.UI
{
    public class OutputPaneList : HostedObjectBase, IOutputPaneList
    {
        private EPaneListOrientation _orientation;
        private List<IOutputPaneSpecifier> _panes;
 
        public OutputPaneList(EPaneListOrientation orientation, IEnumerable<IOutputPaneSpecifier> panes)
        {
            _orientation = orientation;
            if (panes != null)
            {
                _panes = panes.ToList();
            }
        }

        public EPaneListOrientation Orientation
        {
            get { return _orientation; }
        }

        public IList<IOutputPaneSpecifier> Panes
        {
            get { return _panes; }
        }
    }
}
