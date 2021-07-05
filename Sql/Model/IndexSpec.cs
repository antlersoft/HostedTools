using com.antlersoft.HostedTools.Sql.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class IndexSpec : IIndexSpec
    {
        public IndexSpec()
        {

        }
        public IndexSpec(IEnumerable<IIndexColumn> columns)
        {
            _columns.AddRange(columns);
        }

        public IndexSpec AddColumn(IIndexColumn column)
        {
            _columns.Add(column);
            return this;
        }
        private List<IIndexColumn> _columns = new List<IIndexColumn>();
        public IList<IIndexColumn> Columns => _columns;
    }
}
