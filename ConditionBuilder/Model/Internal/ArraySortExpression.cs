using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;
using com.antlersoft.HostedTools.Serialization;

class ArraySortExpression : IOperatorExpression, IGroupExpression
{
    private readonly List<IHtExpression> _operands;
    public string OperatorName => "arraysort";

    public IList<IHtExpression> Operands => _operands;

    public ArraySortExpression(IEnumerable<IHtExpression> expressions)
    {
        _operands = expressions.ToList();
        if (_operands.Count < 1)
        {
            throw new System.ArgumentException("arraycount requires at least 1 argument");
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
        var toSort = _operands[0].Evaluate(context);
        IComparer<IHtValue> comparer;
        ValueComparer valueComparer = new ValueComparer();
        if (_operands.Count > 1)
        {
            comparer = new OrderByComparer(valueComparer, _operands.Skip(1).ToList());
        }
        else
        {
            comparer = valueComparer;
        }

        var result = new JsonHtValue();
        int index = 0;
        foreach (var v in toSort.AsArrayElements.OrderBy(k => k, comparer))
        {
            result[index++] = v;
        }
        return result;
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