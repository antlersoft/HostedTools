using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    public class GroupCount : GroupFunctionBase
    {
        private int _count;

        public GroupCount()
        {
            _count = 0;
        }

        public override void StartGroup()
        {
            base.StartGroup();
            _count = 0;
        }

        public override void AddRow(IHtValue row)
        {
            base.AddRow(row);
            _count++;
        }

        public override IHtValue Evaluate(IHtValue row)
        {
            if (State != GroupFunctionState.End)
            {
                throw new GroupException("GroupCount.Evaluate called before group ended", this);
            }
            return new JsonHtValue(_count);
        }
    }
}