using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using com.antlersoft.HostedTools.Archive.Interface;
using com.antlersoft.HostedTools.Archive.Model.Configuration;
using com.antlersoft.HostedTools.Archive.Model.Serialization;
using com.antlersoft.HostedTools.ConditionBuilder.Interface;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Interface;
using com.antlersoft.HostedTools.Serialization;
using com.antlersoft.HostedTools.Sql.Interface;
using Newtonsoft.Json;

namespace com.antlersoft.HostedTools.Archive.Model
{
    public class FolderRepository : HostedObjectBase, IArchiveRepository
    {
        ISchema _schema;
        string _path;
        IConditionBuilder _builder;
        IJsonFactory _jsonFactory;
        const string SpecFile = "Spec.json";
        const string SchemaFolder = "Schemas";
        const string JsonSuffix = ".json";
        internal const string GzipSuffix = ".gz";
        const int FolderArchiveVersion = 0;
        const string EmptySchema = "{Empty}";
        static char[] _invalid = Path.GetInvalidFileNameChars();

        public FolderRepository(string path, ISchema schema, IConditionBuilder builder = null, IJsonFactory jsonFactory = null)
        {
            _schema = schema;
            _path = path;
            _builder = builder ?? new ConditionBuilder.Model.ConditionBuilder();
            _jsonFactory = jsonFactory ?? new JsonFactory();
        }
        public ISchema Schema => _schema;

        public IArchive GetArchive(IArchiveSpec spec, IWorkMonitor monitor)
        {
            FolderArchiveSpec fa = GetFolderArchiveSpec(spec);
            var path = Path.Combine(_path, fa.Title??"not a folder");
            if (Directory.Exists(path))
            {
                var specPath = Path.Combine(path, SpecFile);
                FolderArchiveSpec specOnDisk = null;
                using (var sr = new StreamReader(specPath))
                using (var jr = new JsonTextReader(sr))
                {
                    specOnDisk = _jsonFactory.GetSerializer().Deserialize<FolderArchiveSpec>(jr);
                    if (! specOnDisk.Equals(fa))
                    {
                        monitor.Writer.WriteLine($"Archive Specification {fa.Title ?? "null"} mismatch on disk");
                    }
                }
                var folderTables = new List<FolderTableArchive>();
                foreach (var t in specOnDisk.TablesInArchive)
                {
                    var tableJson = GetTablePath(t.Schema, t.Name, path);
                    var table = Schema.GetTable(t.Schema, t.Name);
                    if (table == null)
                    {
                        throw new InvalidOperationException($"Schema doesn't have table referenced in Folder Archive: {t.Schema}.{t.Name}");
                    }
                    folderTables.Add(new FolderTableArchive() { Path = tableJson, Table = Schema.GetTable(t.Schema, t.Name) });
                }
                return new FolderArchive(_jsonFactory, this, GetArchiveSpec(specOnDisk), folderTables);
            }
            throw new InvalidOperationException($"No archive found for archive spec {spec.Title}");
        }

        public void WriteArchive(IArchive archive, IWorkMonitor monitor)
        {
            FolderArchiveSpec fa = GetFolderArchiveSpec(archive.Spec);
            fa.TablesInArchive = archive.Tables.Select(
                t => new TableReference() { Name = t.Name, Schema = t.Schema }).ToList();
            string archiveFolderPath = Path.Combine(_path, EscapePath(fa.Title));
            Directory.CreateDirectory(archiveFolderPath);
            string schemaPath = Path.Combine(archiveFolderPath, SpecFile);
            WriteJsonObjectToFile(schemaPath, fa);
            foreach (var ts in archive.Tables)
            {
                var tablePath = GetTablePath(ts, archiveFolderPath);
                if (archive.Spec.UseCompression)
                {
                    using (var fs = new FileStream(tablePath + GzipSuffix, FileMode.Create))
                    using (var gs = new GZipStream(fs, CompressionMode.Compress))
                    {
                        WriteTableToStream(archive.GetRows(ts), gs);
                    }
                }
                else
                {
                    using (var fs = new FileStream(tablePath, FileMode.Create))
                    {
                        WriteTableToStream(archive.GetRows(ts), fs);
                    }
                }
            }
        }

        private void WriteTableToStream(IEnumerable<IHtValue> rows, Stream stream)
        {
            using (StreamWriter s = new StreamWriter(stream))
            using (JsonTextWriter w = new JsonTextWriter(s))
            {
                var serializer = _jsonFactory.GetSerializer(true);
                w.WriteStartArray();
                foreach (var row in rows)
                {
                    w.WriteWhitespace("\n");
                    serializer.Serialize(w, row);
                }
                w.WriteEndArray();
                w.WriteWhitespace("\n");
            }
        }

        public string GetTablePath(ITable table, string archiveFolderPath)
        {
            return GetTablePath(table.Schema, table.Name, archiveFolderPath);
        }

        public string GetTablePath(string schemaName, string tableName, string archiveFolderPath)
        {
            string schemaPath = Path.Combine(archiveFolderPath, SchemaFolder, EscapePath(string.IsNullOrWhiteSpace(schemaName) ? EmptySchema : schemaName));
            Directory.CreateDirectory(schemaPath);
            return Path.Combine(schemaPath, EscapePath(tableName)+JsonSuffix);
        }

        public void WriteJsonObjectToFile(string path, object o)
        {
            using (StreamWriter s = new StreamWriter(path))
            using (JsonTextWriter w = new JsonTextWriter(s))
            {
                _jsonFactory.GetSerializer(true).Serialize(w, o);
            }
        }

        public IEnumerable<IArchiveSpec> AvailableArchives()
        {
            foreach (var folder in Directory.GetDirectories(_path))
            {
                using (var sr = new StreamReader(Path.Combine(folder, SpecFile)))
                using (var jr = new JsonTextReader(sr))
                {
                    yield return GetArchiveSpec(_jsonFactory.GetSerializer().Deserialize<FolderArchiveSpec>(jr));
                }
            }
        }

        private FolderArchiveSpec GetFolderArchiveSpec(IArchiveSpec source)
        {
            FolderArchiveSpec fa = new FolderArchiveSpec();
            fa.Version = FolderArchiveVersion;
            fa.Title = source.Title;
            fa.Tables = new List<FolderTableSpec>();
            fa.UseCompression = source.UseCompression;

            foreach (var t in source.TableSpecs)
            {
                fa.Tables.Add(new FolderTableSpec()
                {
                    SchemaName = t.Table.Schema,
                    TableName = t.Table.Name,
                    FilterExpression = (t.TableFilter as IHtExpressionWithSource)?.ExpressionSource
                });
            }
            return fa;
        }

        private ArchiveSpec GetArchiveSpec(FolderArchiveSpec source)
        {
            return new ArchiveSpec(source.Tables.Select(t =>
            {
                ITable table = _schema.GetTable(t.SchemaName, t.TableName);
                if (table == null)
                {
                    throw new InvalidOperationException($"Couldn't find table in schema matching {t.SchemaName}.{t.TableName}");
                }
                return new ArchiveTableSpec(table, new ExpressionWithSource(_builder, t.FilterExpression));
            }).ToList(), source.Title, source.UseCompression);
        }

        private static string EscapePath(string s)
        {
            StringBuilder sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (_invalid.Contains(c))
                {
                    sb.Append('_');
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
