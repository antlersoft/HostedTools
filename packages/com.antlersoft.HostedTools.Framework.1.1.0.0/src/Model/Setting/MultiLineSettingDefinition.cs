using System;
using com.antlersoft.HostedTools.Framework.Interface.UI;

namespace com.antlersoft.HostedTools.Framework.Model.Setting
{
    public class MultiLineSettingDefinition : SimpleSettingDefinition, IMultiLineValue
    {
        private int _desiredLines;
        public MultiLineSettingDefinition(string name, string scopeKey, int desiredLines, string prompt = null,
            string description = null, Type type = null, string defaultRaw = null, bool useExpansion = true,
            int numberOfPrevious = 20)
            : base(name, scopeKey, prompt, description, type, defaultRaw, useExpansion, numberOfPrevious)
        {
            _desiredLines = desiredLines;
        }

        public int DesiredVisibleLinesOfText
        {
            get { return _desiredLines; }
        }
    }
}
