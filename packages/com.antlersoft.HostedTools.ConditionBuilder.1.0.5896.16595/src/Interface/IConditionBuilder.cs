using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Interface
{
    public interface IConditionBuilder
    {
        IHtExpression ParseCondition(string expr);
    }
}
