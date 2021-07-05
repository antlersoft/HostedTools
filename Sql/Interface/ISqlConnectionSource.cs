using com.antlersoft.HostedTools.Framework.Interface;
using System.Data.Common;

namespace com.antlersoft.HostedTools.Sql.Interface
{
    public interface ISqlConnectionSource : IHostedObject
    {
        /// <summary>
        /// Returns a "new" DbConnection instance.  This connection should be Disposed after a brief lifetime.
        /// </summary>
        DbConnection GetConnection();
    }
}
