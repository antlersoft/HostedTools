namespace com.antlersoft.HostedTools.Interface
{
    public interface IConditionFactory
    {
        IRangeCondition GetRangeCondition(IHtExpression condition);
        IScanCondition GetScanCondition(IHtExpression condition);
        ICheckCondition GetCheckCondition(IHtExpression condition);
        IMultiKeyCondition GetMultiKeyCondition(IHtExpression condition);
    }
}
