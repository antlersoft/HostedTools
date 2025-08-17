using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

class PreviousRowExpression : GroupFunctionBase
{
    private IHtValue _previousRow = new JsonHtValue();
    private IHtValue _currentRow = null;
    private IHtExpression _expression = null;

    public PreviousRowExpression(IEnumerable<IHtExpression> expressions)
    {
        var operands = expressions.ToList();
        if (operands.Count > 1)
        {
            throw new System.ArgumentException("previousrow takes at most 1 argument");
        }
        if (operands.Count == 1)
        {
            _expression = operands[0];
        }
    }

    private void InternalAddRow(IHtValue row)
    {
        row = _expression?.Evaluate(row) ?? row;
        if (_currentRow != null)
        {
            _previousRow = _currentRow;
        }
        _currentRow = row;
    }
    public override void AddRow(IHtValue row)
    {
        base.AddRow(row);
        if (_expression is IGroupExpression group)
        {
            group.AddRow(row);
        }
        InternalAddRow(row);
    }

    public override void StartGroup()
    {
        base.StartGroup();
        if (_expression is IGroupExpression group)
        {
            group.StartGroup();
        }
        _previousRow = new JsonHtValue();
        _currentRow = null;
    }

    public override IHtValue Evaluate(IHtValue data)
    {
        if (State != GroupFunctionState.InGroup)
        {
            InternalAddRow(data);
        }
        return _previousRow;
    }

    public override void EndGroup()
    {
        base.EndGroup();
        if (_expression is IGroupExpression group)
        {
            group.EndGroup();
        }
    }
}