using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;

public class AddedFunctionHolder : AddedFunctionHolderBase, IAddedFunction
{
    public AddedFunctionHolder(Func<IList<IHtValue>, IHtValue> functor, string name, string _namespace=null, string description = null)
    : base(name, _namespace, false, true, description)
    {
        Functor = functor;
    }

    public Func<IList<IHtValue>, IHtValue> Functor { get; private set; }
}