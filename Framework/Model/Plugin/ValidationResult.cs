using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    public class ValidationResult : HostedObjectBase, IValidationResult
    {
        public ValidationResult(IValidatable validated, ISavable context)
        {
            IsValid = true;
            Message = string.Empty;
            Validated = validated;
            Context = context;
            NestedValidations = new List<IValidationResult>();
        }

        public ValidationResult(IValidatable validated, ISavable context, string message)
        {
            IsValid = false;
            Message = message;
            Validated = validated;
            Context = context;
            NestedValidations = new List<IValidationResult>();
        }

        public ValidationResult(IValidatable validated, ISavable context, Exception ex)
        {
            IsValid = false;
            Message = ex.Message;
            Validated = validated;
            Context = context;
            NestedValidations = new List<IValidationResult>();
        }

        public ValidationResult(IValidatable validated, ISavable context, IEnumerable<Tuple<IValidatable,ISavable>> validatables)
        {
            Validated = validated;
            Context = context;
            Message = string.Empty;
            bool isValid = true;
            NestedValidations = new List<IValidationResult>();
            foreach (var validatable in validatables)
            {
                IValidationResult result;
                try
                {
                    result = validatable.Item1.Validate(validatable.Item2);
                }
                catch (Exception ex)
                {
                    result = new ValidationResult(validatable.Item1, validatable.Item2, ex);
                }
                isValid = isValid && result.IsValid;
                if (Message.Length == 0)
                {
                    Message = result.Message;
                }
                NestedValidations.Add(result);
            }
        }

        public ISavable Context {get; private set;}
        public bool IsValid { get; set;}
        public string Message { get; set; }
        public IValidatable Validated { get; private set; }
        public Exception ValidationException {get; set;}
        public IList<IValidationResult> NestedValidations {get; private set;}
    }
}