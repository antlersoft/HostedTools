using System;

namespace com.antlersoft.HostedTools.Framework.Interface.UI
{
    public interface ITextOutput : IHostedObject
    {
        void AddText(string text);
        void Clear();
        void SetFont(Object font);
    }
}
