using System;
using System.Collections.Generic;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class AddedGroupFunctionHolder : AddedFunctionHolderBase, IAddedGroupFunction
    {
        public AddedGroupFunctionHolder(Func<IList<IHtExpression>, IHtExpression> functor, string name, string _namespace=null, bool isGroup = false, bool isImmediate = false, string description = null)
        : base(name, _namespace, isGroup, isImmediate, description)
        {
            Functor = functor;
        }

        public Func<IList<IHtExpression>, IHtExpression> Functor { get; private set; }
    }
}