using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Archive.Model.Configuration;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Interface.Expressions;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql;
using com.antlersoft.HostedTools.Sql.Interface;
using com.antlersoft.HostedTools.Sql.Model;

namespace com.antlersoft.HostedTools.Archive.Model
{
    public class SqlRepository : HostedObjectBase, IArchiveRepository
    {
        /// <summary>
        /// Holds state while building up an incomplete Schema for an SQL repository
        /// </summary>
        class SchemaBuilder
        {
            internal Schema Schema = new Schema();
            internal SqlRepositoryConfiguration JsonConfiguration;
            private HashSet<TableReference> AllTables = new HashSet<TableReference>();
            private HashSet<string> TableNames = new HashSet<string>();
            internal List<Tuple<Table, TableConfiguration>> TablesToConfigure = new List<Tuple<Table, TableConfiguration>>();

            internal bool AddToTableList(TableReference t)
            {
                bool result = AllTables.Add(t);
                TableNames.Add(t.Name.ToLowerInvariant());

                return result;
            }

            internal TableReference FindTableForColumnName(string columnName)
            {
                columnName = columnName.ToLowerInvariant();
                string tableName = null;

                if (TableNames.Contains(columnName))
                {
                    tableName = columnName;
                }
                else
                {
                    string candidate = columnName + "s";
                    if (TableNames.Contains(candidate))
                    {
                        tableName = candidate;
                    }
                    else if (columnName.EndsWith("_id"))
                    {
                        candidate = columnName.Substring(0, columnName.Length - 3);
                        if (TableNames.Contains(candidate))
                        {
                            tableName = candidate;
                        }
                        else
                        {
                            candidate = candidate + "s";
                            if (TableNames.Contains(candidate))
                            {
                                tableName = candidate;
                            }
                        }
                    } else if (columnName.EndsWith("id"))
                    {
                        candidate = columnName.Substring(0, columnName.Length - 2);
                        if (TableNames.Contains(candidate))
                        {
                            tableName = candidate;
                        }
                        else
                        {
                            candidate = candidate + "s";
                            if (TableNames.Contains(candidate))
                            {
                                tableName = candidate;
                            }
                        }

                    }
                }
                TableReference result = null;
                if (tableName != null)
                {
                    result = AllTables.First(t => t.Name.ToLowerInvariant() == tableName);
                }
                return result;
            }
        }
        private ISqlConnectionSource _connectionSource;
        public ISchema Schema { get; }

        public SqlRepository(SqlRepositoryConfiguration jsonConfiguration, ISqlConnectionSource connectionSource)
        {
            _connectionSource = connectionSource;
            var schemaBuilder = new SchemaBuilder() { JsonConfiguration = jsonConfiguration };
            Schema = BuildSchema(schemaBuilder);
        }

        public IArchive GetArchive(IArchiveSpec spec, IWorkMonitor monitor)
        {
            var archiveTables = new List<SqlArchiveTable>();
            var backReferencedTablesToProcess = new List<IArchiveTableSpec>();

            foreach (var t in spec.TableSpecs)
            {
                if (t.ImplicitReferences.Count > 0)
                {
                    backReferencedTablesToProcess.Add(t);
                }
                else
                {
                    var added = AddConstraints(t.Table, archiveTables);
                    added.Filter = t.TableFilter;
                }
            }
            while (backReferencedTablesToProcess.Count > 0)
            {
                var initialCount = backReferencedTablesToProcess.Count;
                var currentList = backReferencedTablesToProcess;
                backReferencedTablesToProcess = new List<IArchiveTableSpec>();
                foreach (var ts in currentList)
                {
                    if (ts.ImplicitReferences.All(ir => archiveTables.Any(at => at.Table == ir)))
                    {
                        var added = AddConstraints(ts.Table, archiveTables);
                        added.Filter = ts.TableFilter;
                        foreach (var ir in ts.ImplicitReferences)
                        {
                            bool newDependents = false;
                            foreach (var c in ts.Table.Constraints)
                            {
                                if (c.ReferencedTable == ir)
                                {
                                    var implicitlyReferenced = archiveTables.First(at => at.Table == ir);
                                    // Remove paths from implicitly referenced to this table,
                                    // which would result in a loop
                                    for (int i = implicitlyReferenced.DependentTables.Count - 1; i>=0; i--)
                                    {
                                        if (implicitlyReferenced.DependentTables[i].Constraint == c)
                                        {
                                            implicitlyReferenced.DependentTables.RemoveAt(i);
                                        }
                                    }
                                    newDependents = true;
                                    added.DependentTables.Add(
                                        new DependentTable()
                                        {
                                            ArchiveTable = implicitlyReferenced,
                                            Constraint = c,
                                            ReverseDependency = true
                                        });
                                }
                            }
                            if (! newDependents)
                            {
                                throw new InvalidOperationException($"Couldn't find constraint for implicit reference from {ts.Table} to {ir}");
                            }
                        }
                    }
                    else
                    {
                        backReferencedTablesToProcess.Add(ts);
                    }
                }
                if (backReferencedTablesToProcess.Count == initialCount)
                {
                    throw new InvalidOperationException("Unable to make progress on back referenced tables");
                }
            }
            return new SqlArchive(spec, _connectionSource, archiveTables);
        }

        private SqlArchiveTable AddConstraints(ITable t, List<SqlArchiveTable> archiveTables)
        {
            var added = archiveTables.FirstOrDefault(a => a.Table == t);
            if (added == null)
            {
                added = new SqlArchiveTable() { Table = t };
                archiveTables.Add(added);
                foreach (var nc in t.Constraints)
                {
                    var nextLevel = AddConstraints(nc.ReferencedTable, archiveTables);
                    if (!nextLevel.DependentTables.Any(dt => dt.ArchiveTable.Table == t && dt.Constraint == nc))
                    {
                        nextLevel.DependentTables.Add(new DependentTable() { ArchiveTable = added, Constraint = nc });
                    }
                }
            }
            return added;
        }

        public void WriteArchive(IArchive archive, IWorkMonitor monitor)
        {
            foreach (var table in TopologicalSort(archive.Tables))
            {
                SqlUtil.InsertRows(_connectionSource, table, archive.GetRows(table).Select(r =>
                {
                    var empty = new JsonHtValue();
                    foreach (var f in table.ForceNullOnInsert)
                    {
                        r[f.Name] = empty;
                    }
                    return r;
                }));
            }
        }

        internal static string GetFilterText(string alias, ITable table, ISqlColumnInfo columnInfo, IHtExpression expression)
        {
            if (expression is IHtExpressionWithSource ews)
            {
                expression = ews.Underlying;
            }
            if (expression is IDataReferenceExpression dataRef)
            {
                return $"{alias}.{columnInfo.GetColumnReference(table[dataRef.ValueName])}";
            }
            else if (expression is IConstantExpression constant)
            {
                var value = constant.Evaluate(new JsonHtValue());
                if (value.IsEmpty)
                {
                    return "null";
                }
                else if (value.IsString)
                {
                    return $"'{value.AsString.Replace("'","''")}'";
                }
                else
                {
                    return value.AsString;
                }
            }
            else if (expression is IOperatorExpression opex)
            {
                if (opex.OperatorName == "IN")
                {
                    StringBuilder inBuilder = new StringBuilder();
                    inBuilder.Append(GetFilterText(alias, table, columnInfo, opex.Operands[0]));
                    inBuilder.Append(" in ");
                    inBuilder.Append(SqlUtil.InClause(opex.Operands.Skip(1).Where(a => a is IConstantExpression).Select(a => ((IConstantExpression)a).Evaluate(null))));
                    return inBuilder.ToString();
                }
                else if (opex.OperatorName == "timestamp" && opex.Operands.Count == 1)
                {
                    return $"timestamp {GetFilterText(alias, table, columnInfo, opex.Operands[0])}";
                }
                else if (opex.Operands.Count == 2)
                {
                    return $"({GetFilterText(alias, table, columnInfo, opex.Operands[0])} {TranslateBinaryOperator(opex.OperatorName)} {GetFilterText(alias, table, columnInfo, opex.Operands[1])})";
                }
            }
            throw new InvalidOperationException($"Unexpected expression type {expression.GetType()}");
        }

        static Dictionary<string, string> Translations = new Dictionary<string, string>()
        {
            {"==", "=" },
            {"!=", "<>" },
            {"&&", "and" },
            {"||", "or" }
        };
        private static string TranslateBinaryOperator(string s)
        {
            string result;
            if (! Translations.TryGetValue(s, out result))
            {
                result = s;
            }
            return result;
        }

        private static List<ITable> TopologicalSort(IEnumerable<ITable> toSort)
        {
            var visited = new HashSet<ITable>();
            var result = new List<ITable>();
            foreach (var t in toSort)
            {
                RecursiveSort(visited, result, t);
            }
            return result;
        }

        private static void RecursiveSort(HashSet<ITable> visited, List<ITable> result, ITable current)
        {
            if (visited.Add(current))
            {
                foreach (var c in current.Constraints)
                {
                    RecursiveSort(visited, result, c.ReferencedTable);
                }
                result.Add(current);
            }
        }

        private static void NormalizeConstraintConfiguration(SchemaBuilder builder, HashSet<string> interestingSchema, List<ConstraintConfiguration> constraints)
        {
            if (constraints != null)
            {
                foreach (var c in constraints)
                {
                    if (string.IsNullOrWhiteSpace(c.ReferencedTable.Schema))
                    {
                        c.ReferencedTable.Schema = builder.JsonConfiguration.DefaultSchema;
                    }
                    else
                    {
                        interestingSchema.Add(c.ReferencedTable.Schema);
                    }
                    builder.AddToTableList(c.ReferencedTable);
                }
            }
        }

        private ISchema BuildSchema(SchemaBuilder builder)
        {
            var interestingSchema = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(builder.JsonConfiguration.DefaultSchema))
            {
                interestingSchema.Add(builder.JsonConfiguration.DefaultSchema);
            }
            if (builder.JsonConfiguration.TableList != null)
            {
                foreach (var t in builder.JsonConfiguration.TableList)
                {
                    if (string.IsNullOrEmpty(t.Table.Schema))
                    {
                        t.Table.Schema = builder.JsonConfiguration.DefaultSchema;
                    }
                    string n = t.Table.Name;
                    string s = t.Table.Schema;
                    Table table = new Table(s, n, t.IsExternal);
                    if (t.Columns != null)
                    {
                        int ordinal = 1;
                        foreach (var c in t.Columns)
                        {
                            table.AddField(new Field(c, "varchar", ordinal++, true));
                        }
                    }
                    builder.TablesToConfigure.Add(new Tuple<Table, TableConfiguration>(table, t));
                    builder.AddToTableList(new TableReference() { Schema = s, Name = n });
                    NormalizeConstraintConfiguration(builder, interestingSchema, t.AddedConstraints);
                    NormalizeConstraintConfiguration(builder, interestingSchema, t.RemovedConstraints);
                }
            }
            var query = "select table_name, table_schema from information_schema.tables where table_schema <> 'information_schema'";
            foreach (var s in builder.JsonConfiguration.TableList.Select(tc => tc.Table.Schema).Where(s => ! string.IsNullOrWhiteSpace(s)))
            {
                interestingSchema.Add(s);
            }
            var completeSchema = new HashSet<string>();
            if (builder.JsonConfiguration.SchemaList != null)
            {
                foreach (var s in builder.JsonConfiguration.SchemaList)
                {
                    interestingSchema.Add(s);
                    if (builder.TablesToConfigure.Count == 0)
                    {
                        completeSchema.Add(s);
                    }
                }
            }
            if (interestingSchema.Count > 0)
            {
                query = query + " and table_schema in " + SqlUtil.InClause(interestingSchema);
            }
            foreach (var row in SqlUtil.GetRows(_connectionSource, query, 5).Select(r => SqlUtil.LowerCaseKeys(r)))
            {
                var reference = new TableReference() { Schema = row["table_schema"].AsString, Name = row["table_name"].AsString };
                if (builder.AddToTableList(reference))
                {
                    if (completeSchema.Contains(reference.Schema))
                    {
                        builder.TablesToConfigure.Add(new Tuple<Table, TableConfiguration>(new Table(reference.Schema, reference.Name), null));
                    }
                }
            }
            while (builder.TablesToConfigure.Count > 0)
            {
                var pair = builder.TablesToConfigure[0];
                BuildTableSchema(builder, pair.Item1, pair.Item2);
                builder.TablesToConfigure.RemoveAt(0);
            }
            return builder.Schema;
        }

        private void PopulateTableFields(Table table)
        {
            if (table.Fields.Count == 0)
            {
                foreach (var row in SqlUtil.GetRows(_connectionSource, $"SELECT * FROM information_schema.columns WHERE table_name = '{table.Name}' AND table_schema = '{table.Schema}' ORDER BY ordinal_position", 5).Select(r => SqlUtil.LowerCaseKeys(r)))
                {
                    IHtValue udt;
                    var dataType = row["data_type"].AsString;
                    if (dataType == "USER-DEFINED" && (udt = row["udt_name"])!=null)
                    {
                        dataType = udt.AsString;
                    }
                    else if (dataType == "bigint" && (udt = row["column_type"])!=null)
                    {
                        dataType = udt.AsString;
                    }
                    table.AddField(new Field(row["column_name"].AsString, dataType, (int)row["ordinal_position"].AsLong, row["is_nullable"].AsBool));
                }
                IIndexSpec primaryKey = null;
                if (_connectionSource.Cast<ISqlPrimaryKeyInfo>() is ISqlPrimaryKeyInfo keyInfo)
                {
                    primaryKey = keyInfo.GetPrimaryKey(table);
                }
                table.SetPrimaryKey(primaryKey);
            }
        }

        private void BuildTableSchema(SchemaBuilder builder, Table table, TableConfiguration config)
        {
            PopulateTableFields(table);
            if (_connectionSource.Cast<ISqlReferentialConstraintInfo>() is ISqlReferentialConstraintInfo constraintGetter)
            {
                foreach (var constraint in constraintGetter.GetReferentialConstraints(table, (schema, name) => GetReferencedTable(builder, schema, name)))
                {
                    AddCandidateConstraint(table, config, new ConstraintConfiguration(constraint), builder);
                }
            }
            if (config?.AddedConstraints != null)
            {
                foreach (var c in config.AddedConstraints)
                {
                    AddCandidateConstraint(table, config, c, builder);
                }
            }
            if (config?.ForceNullOnInsert != null)
            {
                table.SetForceNullOnInsertFields(config.ForceNullOnInsert);
            }
            if (_connectionSource.Cast<ISqlIndexInfo>() is ISqlIndexInfo indexGetter)
            {
                foreach (var indexPair in indexGetter.GetIndexInfo(table))
                {
                    string initialColumn = indexPair.Value.Columns[0].Field.Name;
                    var reference = builder.FindTableForColumnName(initialColumn);
                    if (reference != null)
                    {
                        var referencedTable = GetReferencedTable(builder, reference.Schema, reference.Name);
                        if (referencedTable == table)
                        {
                            continue;
                        }
                        var primary = referencedTable.PrimaryKey;
                        if (primary != null)
                        {
                            List<string> tableColumnNames = new List<string>();
                            List<string> referencedColumnNames = new List<string>();
                            int index = 0;
                            foreach (var indexColSpec in indexPair.Value.Columns)
                            {
                                var indexCol = indexColSpec.Field;
                                if (indexCol == null)
                                {
                                    break;
                                }
                                if (index >= primary.Columns.Count)
                                {
                                    break;
                                }
                                var referencedCol = primary.Columns[index++].Field;
                                if (indexCol.DataType != referencedCol.DataType)
                                {
                                    break;
                                }
                                tableColumnNames.Add(indexCol.Name);
                                referencedColumnNames.Add(referencedCol.Name);
                            }
                            if (tableColumnNames.Count > 0)
                            {
                                AddCandidateConstraint(table, config, new ConstraintConfiguration()
                                {
                                    ReferencedTable = reference,
                                    LocalColumns = tableColumnNames,
                                    ReferencedColumns = referencedColumnNames
                                }, builder);
                            }
                        }
                    }
                }
            }
            builder.Schema.AddTable(table);
        }

        private void AddCandidateConstraint(Table table, TableConfiguration configuration, ConstraintConfiguration candidateConstraint, SchemaBuilder builder)
        {
            if (configuration != null && configuration.RemovedConstraints != null)
            {
                foreach (var cc in configuration.RemovedConstraints)
                {
                    if (cc.IsSameConstraint(candidateConstraint))
                    {
                        return;
                    }
                }
            }
            foreach (var constraint in table.Constraints)
            {
                if (new ConstraintConfiguration(constraint).IsSameConstraint(candidateConstraint))
                {
                    return;
                }
            }
            var referencedTable = GetReferencedTable(builder, candidateConstraint.ReferencedTable.Schema, candidateConstraint.ReferencedTable.Name);
            table.AddConstraint(GetConstraint(candidateConstraint.Name, table, referencedTable, candidateConstraint.LocalColumns, candidateConstraint.ReferencedColumns));
        }

        private Constraint GetConstraint(string constraintName, ITable localTable, ITable referencedTable, IEnumerable<string> localColumnNames, IEnumerable<string> referencedColumnNames)
        {
            if (localColumnNames == null || referencedColumnNames == null)
            {
                throw new InvalidOperationException($"Null column list for constraint [{constraintName ?? string.Empty}] from {localTable.Name} to {referencedTable.Name}");
            }
            var localCol = localColumnNames.ToList();
            var remoteCol = referencedColumnNames.ToList();
            int count = localCol.Count;
            var tableColumns = new List<IndexColumn>();
            var referencedColumns = new List<IndexColumn>();
            if (count != remoteCol.Count)
            {
                throw new InvalidOperationException($"Column count mismatch for constraint [{constraintName ?? string.Empty}] from {localTable.Name} to {referencedTable.Name}");
            }
            for (int i=0; i<count; i++)
            {
                var column = localTable[localCol[i]];
                if (column == null)
                {
                    throw new InvalidOperationException($"No column {localCol[i]} in  ${localTable.Schema}.{localTable.Name}");
                }
                tableColumns.Add(new IndexColumn(column));
                column = referencedTable[remoteCol[i]];
                if (column == null)
                {
                    throw new InvalidOperationException($"No column {remoteCol[i]} in  ${referencedTable.Schema}.{referencedTable.Name}");
                }
                referencedColumns.Add(new IndexColumn(column));
            }
            return new Constraint(constraintName, referencedTable, new IndexSpec(tableColumns), new IndexSpec(referencedColumns));
        }

        /// <summary>
        /// Return an ITable instance guaranteed to be populated with columns, although not with constraints
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private ITable GetReferencedTable(SchemaBuilder builder, string schemaName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                schemaName = builder.JsonConfiguration.DefaultSchema;
            }
            TableReference reference = new TableReference() { Schema = schemaName, Name = tableName };
            builder.AddToTableList(reference);
            var referencedTable = builder.Schema.GetTable(schemaName, tableName);
            if (referencedTable == null)
            {
                var pair = builder.TablesToConfigure.FirstOrDefault(c => (new TableReference() { Schema = c.Item1.Schema, Name = c.Item1.Name }).Equals(reference));
                if (pair != null)
                {
                    referencedTable = pair.Item1;
                    PopulateTableFields(pair.Item1);
                }
                else
                {
                    var newTable = new Table(reference.Schema, reference.Name);
                    builder.TablesToConfigure.Add(new Tuple<Table, TableConfiguration>(newTable, null));
                    PopulateTableFields(newTable);
                    referencedTable = newTable;
                }
            }
            return referencedTable;
        }
    }
}
