using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.ConditionBuilder.Parser
{
    public class InFixNode : IParseTreeNode
    {
        public InFixNode(Type resultType, InFixOperator op, IParseTreeNode left, IParseTreeNode right)
        {
            ResultType = resultType;
            Operator = op;
            LeftNode = left;
            RightNode = right;
        }

        public InFixOperator Operator { get; private set; }
        public IParseTreeNode LeftNode { get; set; }
        public IParseTreeNode RightNode { get; private set; }

        public Type ResultType
        {
            get;
            private set;
        }

        /// <summary>
        /// Called when precedence for infix operators in parse tree have been resolved, to
        /// make sure operator arguments are matched up
        /// </summary>
        /// <param name="nextOffset">Value only matters on top-level call</param>
        /// <returns>GoalResult of failure goal, or success</returns>
        public GoalResult ResolveTypes(int nextOffset)
        {
            // Depth first traversal
            if (LeftNode is InFixNode)
            {
                GoalResult gr = ((InFixNode)LeftNode).ResolveTypes(0);
                if (!gr.Succeeded)
                {
                    return gr;
                }
            }
            if (RightNode is InFixNode)
            {
                GoalResult gr = ((InFixNode)RightNode).ResolveTypes(0);
                if (!gr.Succeeded)
                {
                    return gr;
                }
            }
            OperatorValidity ov = Operator.GetReturnType(LeftNode.ResultType, RightNode.ResultType);
            if (!ov.IsValid)
            {
                return new GoalResult(ov.Message);
            }
            ResultType = ov.ResultType;
            return new GoalResult(this, nextOffset);
        }

        public Func<object, object> GetFunctor()
        {
            Func<object, object> leftFunctor = LeftNode.GetFunctor();
            Func<object, object> rightFunctor = RightNode.GetFunctor();
            return d => Operator.Evaluate(d, leftFunctor, rightFunctor);
        }
    }
}
