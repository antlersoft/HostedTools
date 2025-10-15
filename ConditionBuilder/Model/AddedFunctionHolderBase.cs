using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Model;

public class AddedFunctionHolderBase : HostedObjectBase, IFunctionInfo
{
    public AddedFunctionHolderBase(string name, string _namespace=null, bool isGroup = false, bool isImmediate = false, string description = null)
    {
        Name = name;
        Namespace = _namespace == null ? string.Empty : _namespace;
        IsGroup = isGroup;
        IsImmediate = isImmediate;
        Description = description == null ? string.Empty : description;
    }
    public string Name { get; private set; }

    public string Namespace { get; private set; }

    public bool IsGroup { get; private set; }

    public bool IsImmediate { get; private set; }

    public string Description { get; private set; }
}