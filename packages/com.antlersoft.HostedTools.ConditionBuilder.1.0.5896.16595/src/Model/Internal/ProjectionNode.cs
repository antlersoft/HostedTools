using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.ConditionBuilder.Parser;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class ProjectionNode : IParseTreeNode
    {
        private readonly List<ProjectionItemNode> _items;
        private readonly bool _copyRemaining;

        internal ProjectionNode(ProjectionNode prev, ProjectionItemNode added)
        {
            _items = new List<ProjectionItemNode>(prev._items.Count + 1);
            _items.Add(added);
            _items.AddRange(prev._items);
            _copyRemaining = prev._copyRemaining;
        }

        internal ProjectionNode(bool copyRemaining)
        {
            _items = new List<ProjectionItemNode>();
            _copyRemaining = copyRemaining;
        }

        public Func<object, object> GetFunctor()
        {
            return d => new ProjectionExpression(_copyRemaining, _items.Select(
                item =>
                    new Tuple<IHtExpression, IHtExpression>((IHtExpression) item.NameNode.GetFunctor()(d),
                        item.ValueNode == null ? null : (IHtExpression) item.ValueNode.GetFunctor()(d))));
        }

        public Type ResultType
        {
            get { return typeof(IHtValue); }
        }
    }
}
