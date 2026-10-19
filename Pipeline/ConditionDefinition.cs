
using System;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Setting;

namespace com.antlersoft.HostedTools.Pipeline
{
    public class ConditionDefinition : SimpleSettingDefinition, IValidatable
    {
        public ConditionDefinition(string name, string scopeKey = "", string prompt = null,
                                string description = null, Type type = null,
                                string defaultRaw = null, bool useExpansion = true, int numberOfPrevious = 10)
        : base(name, scopeKey, prompt, description, type, defaultRaw, useExpansion, numberOfPrevious)
        {
        }

        public IValidationResult Validate(ISavable evaluationContext)
        {
            return ConditionValidation.ConditionValidatable.Validate(this, evaluationContext);
        }
    }
}