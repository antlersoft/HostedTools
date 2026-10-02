using com.antlersoft.HostedTools.Framework.Interface.Plugin;

namespace com.antlersoft.HostedTools.Framework.Model.Plugin
{
    public class ValidationResult : HostedObjectBase, IValidationResult
    {
        public bool IsValid { get; set;}
        public string Message { get; set; }
    }
}