using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface ITextOutput : IHostedObject
    {
        void AddText(string text);
        void Clear();
        void SetFont(Font font);
    }
}
