using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Model.UI {
    /// <summary>
    /// Simple implementation of an always-enabled grid command item that invokes an Action
    /// </summary>
    public class SimpleGridItemCommand : IGridItemCommand
    {
        private readonly string _prompt;
        private readonly Action<Dictionary<string,object>,string> _action;
        public string Prompt => _prompt;

        public SimpleGridItemCommand(string prompt, Action<Dictionary<string,object>,string> action) {
            _prompt = prompt;
            _action = action;
        }

        public void Invoke(Dictionary<string, object> row, string column)
        {
            _action(row, column);
        }

        public bool IsEnabled(Dictionary<string, object> row, string column)
        {
            return true;
        }
    }
}