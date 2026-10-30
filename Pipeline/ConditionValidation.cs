using System;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Plugin;

namespace com.antlersoft.HostedTools.Pipeline
{
    [Export]
    [Export(typeof(IAfterComposition))]
    public class ConditionValidation : HostedObjectBase, IValidatable, IAfterComposition
    {
        [Import]
        public IConditionBuilder ConditionBuilder;

        internal IValidationResult Validate(IValidatable container, ISavable savable)
        {
            string validationText = savable.Cast<ISettingEditor>()?.RawValue??string.Empty;
            try
            {
                ConditionBuilder.ParseCondition(validationText);
            }
            catch (Exception ex)
            {
                return new ValidationResult(container, savable, ex);
            }
            return new ValidationResult(container, savable);
        }

        public IValidationResult Validate(ISavable savable)
        {
            return Validate(this, savable);
        }

        public void AfterComposition()
        {
            _validatable = this;
        }

        private static ConditionValidation _validatable;
        public static ConditionValidation ConditionValidatable
        {
            get
            {
                if (_validatable == null)
                {
                    throw new CompositionException("Attempt to use static ConditionValidatable before composition complete");
                }
                return _validatable;
            }
        }
    }
}