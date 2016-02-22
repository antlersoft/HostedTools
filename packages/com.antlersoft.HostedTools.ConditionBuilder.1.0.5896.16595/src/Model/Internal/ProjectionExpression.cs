using System;
using System.Collections.Generic;
using System.Linq;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;

namespace com.antlersoft.HostedTools.ConditionBuilder.Model.Internal
{
    class ProjectionExpression : IHtExpression
    {
        private readonly List<Tuple<IHtExpression, IHtExpression>> _projectionItems;
        private readonly bool _copyRemaining;

        internal ProjectionExpression(bool copyRemaining,
            IEnumerable<Tuple<IHtExpression, IHtExpression>> projectionItems)
        {
            _copyRemaining = copyRemaining;
            _projectionItems = projectionItems.ToList();
        }

        public IHtValue Evaluate(IHtValue data)
        {
            IHtValue result = new JsonHtValue();
            HashSet<string> used = new HashSet<string>();
            foreach (var item in _projectionItems)
            {
                if (item.Item2 is WildCardExpression)
                {
                    IHtValue dictionary = item.Item1.Evaluate(data);
                    if (dictionary.IsDictionary)
                    {
                        foreach (var kvp in dictionary.AsDictionaryElements)
                        {
                            if (! used.Contains(kvp.Key))
                            {
                                result[kvp.Key] = kvp.Value;
                                used.Add(kvp.Key);
                            }
                        }
                    }
                }
                else
                {
                    string name = item.Item1.Evaluate(data).AsString;
                    result[name] = item.Item2 == null ? data[name] : item.Item2.Evaluate(data);
                    used.Add(name);
                }
            }
            if (_copyRemaining)
            {
                foreach (var kvp in data.AsDictionaryElements)
                {
                    if (! used.Contains(kvp.Key))
                    {
                        result[kvp.Key] = kvp.Value;
                    }
                }
            }
            return result;
        }
    }
}
