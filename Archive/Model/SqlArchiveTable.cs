using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql;
using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.antlersoft.HostedTools.Archive.Model
{
    class SqlArchiveTable
    {
        internal ITable Table { get; set; }
        internal IHtExpression Filter { get; set; }
        internal List<DependentTable> DependentTables { get; } = new List<DependentTable>();
        internal string GetQuery(SqlArchive archive, IDistinctHandling distinctHandling, ISqlColumnInfo columnInfo)
        {
            var unionPaths = new List<List<DependentTable>>();
            var start = new List<DependentTable>();
            unionPaths.Add(start);
            AddDistinctPaths(start, unionPaths);
            /*
            int numberOfPaths = unionPaths.Count;
            var backLinksPerPath = new List<IConstraint>[numberOfPaths];
            for (int i = 0; i<numberOfPaths; i++)
            {
                var backLinks = new List<IConstraint>();
                backLinksPerPath[i] = backLinks;
                foreach (var link in unionPaths[i])
                {
                    if (link.ReverseDependency)
                    {
                        backLinks.Add(link.Constraint);
                    }
                }
            }
            int maxCount = backLinksPerPath.Max(bl => bl.Count);
            List<IConstraint> maxConstraints = backLinksPerPath.First(bl => bl.Count == maxCount);
            unionPaths = unionPaths.Where(p => maxConstraints.All(c => p.Select(dt => dt.Constraint).Contains(c)) || !p.Any(c => c.ReverseDependency)).ToList();
            */
            var alias = new TableAlias();
            StringBuilder totalQuery = new StringBuilder();
            foreach (var path in unionPaths)
            {
                if (totalQuery.Length > 0)
                {
                    totalQuery.Append("\r\nunion\r\n");
                }
                totalQuery.Append(SinglePathQuery(archive, distinctHandling, columnInfo, alias, path));
            }
            return totalQuery.ToString();
        }

        private string SinglePathQuery(SqlArchive archive, IDistinctHandling distinctHandling, ISqlColumnInfo columnInfo, TableAlias alias, List<DependentTable> path)
        {
            string myAlias = alias.Next;
            StringBuilder query = new StringBuilder();
            StringBuilder whereBuilder = new StringBuilder();

            if (columnInfo == null)
            {
                columnInfo = new SimpleColumnInfo();
            }
            string columnList;
            if (Table.RequiredColumnListInSelect)
            {
                StringBuilder colBuilder = new StringBuilder();
                foreach (var c in Table.Fields)
                {
                    if (colBuilder.Length > 0)
                    {
                        colBuilder.Append(',');
                    }
                    colBuilder.Append(myAlias);
                    colBuilder.Append('.');
                    colBuilder.Append(c.Name);
                }
                columnList = colBuilder.ToString();
            }
            else
            {
                columnList = $"{myAlias}.*";
            }

            query.Append($"select {distinctHandling?.GetDistinctText(new[] { new Tuple<string, ITable>(myAlias, Table) })??"distinct"} {columnList} from {Table.Schema}.{Table.Name} {myAlias}");
            if (Filter != null)
            {
                whereBuilder.Append(SqlRepository.GetFilterText(myAlias, Table, columnInfo, Filter));
            }
            var currentAlias = myAlias;
            foreach (var dt in path)
            {
                int columns = dt.Constraint.LocalColumns.Columns.Count;
                if (dt.Constraint.Cast<ISpecialColumnValueGetter>() is ISpecialColumnValueGetter getter)
                {
                    if (columns != 1)
                    {
                        throw new Exception("No support for multi-column special value in constraint");
                    }
                    if (dt.ReverseDependency)
                    {
                        throw new Exception("No support for reverse dependency to special column get value");
                    }
                    var inValues = new HashSet<string>();
                    bool allNumeric = true;
                    foreach (var row in archive.GetCachedRows(dt.ArchiveTable))
                    {
                        foreach (var rowValue in getter.GetColumnValueSets(dt.Constraint.LocalColumns, row))
                        {
                            foreach (var v in rowValue.Values)
                            {
                                if (! v.IsLong && ! v.IsDouble)
                                {
                                    allNumeric = false;
                                }
                                inValues.Add(v.AsString);
                            }
                        }
                    }
                    if (whereBuilder.Length > 0)
                    {
                        whereBuilder.Append("\r\nand ");
                    }
                    if (inValues.Count == 0)
                    {
                        whereBuilder.Append("0 = 1");
                    }
                    else
                    {
                        whereBuilder.Append($"{currentAlias}.{dt.Constraint.ReferencedColumns.Columns[0].Field.Name} in (");
                        bool first = true;
                        foreach (var v in inValues)
                        {
                            if (first)
                            {
                                first = false;
                            }
                            else
                            {
                                whereBuilder.Append(',');
                            }
                            if (! allNumeric)
                            {
                                whereBuilder.Append('\'');
                                foreach (char c in v)
                                {
                                    if (c == '\'')
                                    {
                                        whereBuilder.Append('\'');
                                    }
                                    whereBuilder.Append(c);
                                }
                                whereBuilder.Append('\'');
                            }
                            else
                            {
                                whereBuilder.Append(v);
                            }
                        }
                        whereBuilder.Append(')');
                    }
                    break; // Don't consider further dt's
                }
                else
                {
                    var prevAlias = currentAlias;
                    currentAlias = alias.Next;
                    query.Append($"\r\njoin {dt.ArchiveTable.Table.Schema}.{dt.ArchiveTable.Table.Name} {currentAlias} on ");
                    for (int i = 0; i < columns; i++)
                    {
                        if (i > 0)
                        {
                            query.Append(" and ");
                        }
                        if (dt.ReverseDependency)
                        {
                            query.Append($"{prevAlias}.{columnInfo.GetColumnReference(dt.Constraint.LocalColumns.Columns[i].Field)}={currentAlias}.{columnInfo.GetColumnReference(dt.Constraint.ReferencedColumns.Columns[i].Field)}");
                        }
                        else
                        {
                            query.Append($"{prevAlias}.{columnInfo.GetColumnReference(dt.Constraint.ReferencedColumns.Columns[i].Field)}={currentAlias}.{columnInfo.GetColumnReference(dt.Constraint.LocalColumns.Columns[i].Field)}");
                        }
                    }
                    if (dt.ArchiveTable.Filter != null)
                    {
                        if (whereBuilder.Length > 0)
                        {
                            whereBuilder.Append("\r\nand ");
                        }
                        whereBuilder.Append(SqlRepository.GetFilterText(currentAlias, dt.ArchiveTable.Table, columnInfo, dt.ArchiveTable.Filter));
                    }
                }
            }
            if (whereBuilder.Length > 0)
            {
                query.Append("\r\nwhere ");
                query.Append(whereBuilder);
            }
            return query.ToString();
        }

        private void AddDistinctPaths(List<DependentTable> pathToHere, List<List<DependentTable>> unionPaths)
        {
            var noLoops = DependentTables.Where(dt => pathToHere.All(pt => pt.ArchiveTable != dt.ArchiveTable)).ToList();
            var dc = noLoops.Count;
            for (int i = dc-1; i>=0; i--)
            {
                var currentPath = pathToHere;
                if (i > 0)
                {
                    currentPath = pathToHere.ToList();
                    unionPaths.Add(currentPath);
                }
                currentPath.Add(noLoops[i]);
                noLoops[i].ArchiveTable.AddDistinctPaths(currentPath, unionPaths);
            }
        }

        public override string ToString()
        {
            return $"Archive of {Table.Schema}.{Table.Name}";
        }
    }
}
