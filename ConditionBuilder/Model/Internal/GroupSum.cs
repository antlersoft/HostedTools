using System.Runtime.Serialization;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public class GroupSum : GroupFunctionBase
    {
        private double _sum;
        private IHtExpression _sumOver;

        public GroupSum(IHtExpression sumOver)
        {
            _sumOver = sumOver;
        }

        public override void StartGroup()
        {
            base.StartGroup();
            _sum = 0;
        }

        public override void AddRow(IHtValue row)
        {
            base.AddRow(row);
            _sum += _sumOver!=null ? _sumOver.Evaluate(row).AsDouble : row.AsDouble;
        }

        public override IHtValue Evaluate(IHtValue data)
        {
            if (State != GroupFunctionState.End)
            {
                throw new GroupException("GroupSum.Evaluate called before group ended", this);
            }
            return new JsonHtValue(_sum);
        }
    }
}