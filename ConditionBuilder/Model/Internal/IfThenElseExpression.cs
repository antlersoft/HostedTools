using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    public class IfThenElseExpression : IOperatorExpression, IGroupExpression
    {
        private readonly List<IHtExpression> _operands;
        public string OperatorName => "ifthenelse";
    
        public IList<IHtExpression> Operands => _operands;

        public IfThenElseExpression(IEnumerable<IHtExpression> expressions)
        {
            _operands = expressions.ToList();
            if (_operands.Count != 3)
            {
                throw new System.ArgumentException("ifthenelse requires 3 arguments");
            }
        }

        public void AddRow(IHtValue row)
        {
            foreach (var exp in _operands)
            {
                if (exp is IGroupExpression group)
                {
                    group.AddRow(row);
                }
            }
        }

        public void EndGroup()
        {
            foreach (var exp in _operands)
            {
                if (exp is IGroupExpression group)
                {
                    group.EndGroup();
                }
            }    
        }

        public IHtValue Evaluate(IHtValue context)
        {
            return _operands[0].Evaluate(context).AsBool ? _operands[1].Evaluate(context) : _operands[2].Evaluate(context);
        }

        public void StartGroup()
        {
            foreach (var exp in _operands)
            {
                if (exp is IGroupExpression group)
                {
                    group.StartGroup();
                }
            }
        }
    }
}