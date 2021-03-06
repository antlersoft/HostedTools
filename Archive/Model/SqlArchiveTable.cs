﻿using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Sql;
using com.antlersoft.HostedTools.Sql.Interface;
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
        internal string Query {
            get {
                var unionPaths = new List<List<DependentTable>>();
                var start = new List<DependentTable>();
                unionPaths.Add(start);
                AddDistinctPaths(start, unionPaths);
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
                unionPaths = unionPaths.Where(p => maxConstraints.All(c => p.Select(dt => dt.Constraint).Contains(c))).ToList();
                var alias = new TableAlias();
                StringBuilder totalQuery = new StringBuilder();
                foreach (var path in unionPaths)
                {
                    if (totalQuery.Length > 0)
                    {
                        totalQuery.Append("\r\nunion\r\n");
                    }
                    totalQuery.Append(SinglePathQuery(alias, path));
                }
                return totalQuery.ToString();
            }
        }

        private string SinglePathQuery(TableAlias alias, List<DependentTable> path)
        {
            string myAlias = alias.Next;
            StringBuilder query = new StringBuilder();
            StringBuilder whereBuilder = new StringBuilder();

            query.Append($"select distinct {myAlias}.* from {Table.Schema}.{Table.Name} {myAlias}");
            if (Filter != null)
            {
                whereBuilder.Append(SqlRepository.GetFilterText(myAlias, Filter));
            }
            var currentAlias = myAlias;
            foreach (var dt in path)
            {
                var prevAlias = currentAlias;
                currentAlias = alias.Next;
                int columns = dt.Constraint.LocalColumns.Columns.Count;
                query.Append($"\r\njoin {dt.ArchiveTable.Table.Schema}.{dt.ArchiveTable.Table.Name} {currentAlias} on ");
                for (int i = 0; i<columns; i++)
                {
                    if (i > 0)
                    {
                        query.Append(" and ");
                    }
                    if (dt.ReverseDependency)
                    {
                        query.Append($"{prevAlias}.{dt.Constraint.LocalColumns.Columns[i].Field.Name}={currentAlias}.{dt.Constraint.ReferencedColumns.Columns[i].Field.Name}");
                    }
                    else
                    {
                        query.Append($"{prevAlias}.{dt.Constraint.ReferencedColumns.Columns[i].Field.Name}={currentAlias}.{dt.Constraint.LocalColumns.Columns[i].Field.Name}");
                    }
                }
                if (dt.ArchiveTable.Filter != null)
                {
                    if (whereBuilder.Length > 0)
                    {
                        whereBuilder.Append("\r\nand ");
                    }
                    whereBuilder.Append(SqlRepository.GetFilterText(currentAlias, dt.ArchiveTable.Filter));
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
