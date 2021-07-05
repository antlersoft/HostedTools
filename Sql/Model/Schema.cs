using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Framework.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace com.antlersoft.HostedTools.Sql.Model
{
    public class Schema : HostedObjectBase, ISchema
    {
        List<ITable> _tables = new List<ITable>();
        public IList<ITable> Tables => _tables;

        public Schema AddTable(ITable t)
        {
            _tables.Add(t);
            return this;
        }

        public ITable GetTable(string schemaName, string tableName)
        {
            foreach (var t in _tables)
            {
                if (t.Name == tableName && t.Schema == schemaName)
                {
                    return t;
                }
            }
            return null;
        }
    }
}
