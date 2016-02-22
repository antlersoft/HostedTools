using System;
namespace com.antlersoft.HostedTools.Interface
{
    public interface IHtExpression
    {
        IHtValue Evaluate(IHtValue data);
    }
}

