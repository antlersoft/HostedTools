using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface IGridOutput : IHostedObject
    {
        void AddRow(Dictionary<string, object> row);
        void Clear();
    }
}
