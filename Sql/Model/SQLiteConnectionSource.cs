using System.Data.Common;
using System.Data.SQLite;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Sql.Interface;

namespace com.antlersoft.HostedTools.Sql.Model {
    public class SQLiteConnectionSource : HostedObjectBase, ISqlConnectionSource
    {
        private readonly string _connectionString;

        public SQLiteConnectionSource(string path) {
            _connectionString = path;
        }
        public DbConnection GetConnection()
        {
            var result = new SQLiteConnection($"DataSource={_connectionString}");
            result.Open();
            return result;
        }
    }
}