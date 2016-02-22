using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class OperatorExpression : IOperatorExpression
    {
        private readonly Func<IList<IHtValue>, IHtValue> _evaluator;
        private readonly string _name;
        private readonly List<IHtExpression> _operands; 
        public OperatorExpression(string name, Func<IList<IHtValue>, IHtValue> evaluator,
            IEnumerable<IHtExpression> args)
        {
            _name = name;
            _evaluator = evaluator;
            _operands = args.ToList();
        }

        public string OperatorName
        {
            get { return _name; }
        }

        public IList<IHtExpression> Operands
        {
            get { return _operands; }
        }

        public virtual IHtValue Evaluate(IHtValue data)
        {
            return _evaluator(_operands.Select(e => e.Evaluate(data)).ToList());
        }
    }
}
