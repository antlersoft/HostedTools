using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;

namespace com.antlersoft.HostedTools.Archive.Model
{
    public class DateArchiveFilter : HostedObjectBase, IArchiveFilter
    {
        internal class DateFilteredArchive : IArchive
        {
            private IArchive _underlying;
            private DateArchiveFilter _filter;

            internal DateFilteredArchive(DateArchiveFilter filter, IArchive underlying)
            {
                _filter = filter;
                _underlying = underlying;
            }

            public IArchiveSpec Spec => _underlying.Spec;

            public IEnumerable<ITable> Tables => _underlying.Tables;

            public T Cast<T>(bool fromAggregated = false) where T : class
            {
                return _underlying.Cast<T>(fromAggregated);
            }

            public IEnumerable<IHtValue> GetRows(ITable table)
            {
                long offset = _filter.Offset.Ticks;

                foreach (var row in _underlying.GetRows(table))
                {
                    if (row != null && row.IsDictionary)
                    {
                        foreach (var col in row.AsDictionaryElements)
                        {
                            if (col.Value.IsDictionary)
                            {
                                var v = col.Value;
                                var ticks = v["Ticks"];
                                if (ticks!=null && ticks.IsLong)
                                {
                                    v["Ticks"] = new JsonHtValue(ticks.AsLong + offset);
                                }
                            }
                        }
                    }
                    yield return row;
                }
            }
        }

        public TimeSpan Offset { get; set; }
        public IArchive GetFilteredArchive(IArchive underlying)
        {
            return new DateFilteredArchive(this, underlying);
        }
    }
}
