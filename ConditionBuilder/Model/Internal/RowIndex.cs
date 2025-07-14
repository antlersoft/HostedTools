using com.antlersoft.HostedTools.ConditionBuilder.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

class RowIndex : GroupFunctionBase
{
    int _rowIndex=0;

    public override void AddRow(IHtValue row)
    {
        base.AddRow(row);
        _rowIndex++;
    }

    public override void StartGroup()
    {
        base.StartGroup();
        _rowIndex = 0;
    }

    public override IHtValue Evaluate(IHtValue data)
    {
        var result = new JsonHtValue(_rowIndex);

        if (this.State != GroupFunctionState.InGroup)
        {
            _rowIndex++;
        }

        return result;
    }
}