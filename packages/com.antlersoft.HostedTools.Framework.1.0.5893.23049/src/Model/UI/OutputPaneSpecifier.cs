using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Model.UI
{
    public class OutputPaneSpecifier : HostedObjectBase, IOutputPaneSpecifier
    {
        public OutputPaneSpecifier(EOutputPaneType type, string title = null, int proportion = 0,
                                   IOutputPaneList nestedPanes = null)
        {
            Type = type;
            Title = title;
            Proportion = proportion;
            NestedPanes = nestedPanes;
        }

        public EOutputPaneType Type
        {
            get; private set;
        }

        public string Title
        {
            get; private set;
        }

        public int Proportion
        {
            get; private set;
        }

        public IOutputPaneList NestedPanes
        {
            get; private set;
        }
    }
}
