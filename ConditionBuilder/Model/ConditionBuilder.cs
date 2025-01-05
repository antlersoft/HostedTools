using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model.Internal;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    [Export(typeof(IConditionBuilder))]
    public class ConditionBuilder : IConditionBuilder
    {
        [ImportMany]
        public IEnumerable<IFunctionSource> FunctionSources { get; set; }
        private ConditionParser _parser;

        private ConditionParser ConditionParser {
            get {
                if (_parser == null) {
                    _parser = new ConditionParser(FunctionSources);
                }
                return _parser;
            }
        }

        public IHtExpression ParseCondition(string expr)
        {
            if (string.IsNullOrEmpty(expr))
            {
                return null;
            }
            return (IHtExpression) ConditionParser.ParseExpression(ConditionParser.ExprSym, typeof (bool), expr)(this);
        }

        public IHtExpression ParseConditionVerbose(string expr, TextWriter writer)
        {
            if (string.IsNullOrEmpty(expr))
            {
                return null;
            }
            return (IHtExpression)ConditionParser.ParseExpression(ConditionParser.ExprSym, typeof(bool), expr, writer)(this);            
        }
    }
}
