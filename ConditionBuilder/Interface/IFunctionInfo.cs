
using com.antlersoft.HostedTools.Framework.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface
{
    public interface IFunctionInfo : IHostedObject
    {
        string Name { get; }
        string Namespace { get; }
        /// <summary>
        /// True if works as group function
        /// </summary>
        bool IsGroup { get; }
        /// <summary>
        /// True if works in non-group context
        /// </summary>
        bool IsImmediate { get; }
        string Description { get; }
    }
}