namespace com.antlersoft.HostedTools.Framework.Interface.Plugin
{
    public interface IValidationResult : IHostedObject
    {
        bool IsValid { get; }
        string Message { get; }
    }
    public interface IValidatable : IHostedObject
    {
        IValidationResult Validate(ISavable evaluationContext);
    }
}