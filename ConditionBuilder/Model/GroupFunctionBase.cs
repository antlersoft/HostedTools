using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model
{
    public abstract class GroupFunctionBase : IHtExpression, IGroupExpression
    {
        public enum GroupFunctionState {
            Start,
            InGroup,
            End
        }

        public GroupFunctionState State { get; set; } = GroupFunctionState.Start;

        public abstract IHtValue Evaluate(IHtValue data);
        public virtual void StartGroup() {
            State = GroupFunctionState.InGroup;
        }
        public virtual void AddRow(IHtValue row) {
            if (State != GroupFunctionState.InGroup) {
                throw new GroupException("AddRow called when not in group", this);
            }                
        }
        public virtual void EndGroup() {
            if (State != GroupFunctionState.InGroup) {
                throw new GroupException("EndGroup called when not in group", this);
            }
            State = GroupFunctionState.End;
        }

    }
}