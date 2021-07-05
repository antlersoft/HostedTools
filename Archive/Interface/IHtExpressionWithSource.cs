using com.antlersoft.HostedTools.Framework.Interface;
using com.antlersoft.HostedTools.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Interface
{
    public interface IHtExpressionWithSource : IHostedObject, IHtExpression
    {
        string ExpressionSource { get; }
        IHtExpression Underlying { get; }
    }
}
