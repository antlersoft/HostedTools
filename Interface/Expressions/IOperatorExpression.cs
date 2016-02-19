using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Interface.Expressions
{
    public interface IOperatorExpression : IHtExpression
    {
        string OperatorName { get; }
        IList<IHtExpression> Operands { get; } 
    }
}
