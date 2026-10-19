using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface IValidationResult : IHostedObject
    {
        IValidatable Validated { get; }
        ISavable Context {get;}
        bool IsValid { get; }
        string Message { get; }
        IList<IValidationResult> NestedValidations { get; }
        Exception ValidationException { get; }
    }

    public interface IValidatable : IHostedObject
    {
        IValidationResult Validate(ISavable evaluationContext);
    }
}