using com.antlersoft.HostedTools.Interface;

class SameExpression : IHtExpression
{
    public IHtValue Evaluate(IHtValue data)
    {
        return data;
    }
}