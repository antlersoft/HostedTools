using System;
using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model.Plugin;

public static class ValidationExtensions
{
    public class DummyValidation : IValidatable
    {
        public IValidationResult Validate(ISavable savable)
        {
            return new ValidationResult(this, savable);
        }

        public T Cast<T>(bool fromAggregated = false) where T : class
        {
            return this as T;
        }

        public static readonly DummyValidation Instance = new DummyValidation();
    }

    public static IValidationResult ValidateSavable(this ISavable savable, IHostedObject candidateValidator)
    {
        IValidatable validatable = candidateValidator?.Cast<IValidatable>();
        return (validatable??DummyValidation.Instance).Validate(savable);
    }
}