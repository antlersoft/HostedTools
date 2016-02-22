using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Interface
{
    public interface INoSql : IDisposable
    {
        IHtValue GetValue(string table, IHtValue hashKey, IHtValue rangeKey = null, bool requireConsistent = false);
        Task<IHtValue> GetValueAsync(string table, IHtValue hashKey, IHtValue rangeKey = null, bool requireConsistent = false);
        IEnumerable<IHtValue> BatchGetValue(string table, IEnumerable<IHtValue> keys, bool requireConsistent = false);
        Task<IAsyncBatcher<IHtValue>> BatchGetValueAsync(string table, IEnumerable<IHtValue> keys, bool requireConsistent = false);
        void SetValue(string table, IHtValue hashKey, IHtValue rangeKey, IHtValue row, bool replace=true);
        Task SetValueAsync(string table, IHtValue hashKey, IHtValue rangeKey, IHtValue row, bool replace=true);
        bool ConditionalSetValue(string table, IHtValue hashKey, IHtValue rangeKey, IHtValue row, ICheckCondition toCheck, bool replace = true);
        Task<bool> ConditionalSetValueAsync(string table, IHtValue hashKey, IHtValue rangeKey, IHtValue row, ICheckCondition toCheck, bool replace = true);
        void DeleteValue(string table, IHtValue hashKey, IHtValue rangeKey);
        Task DeleteValueAsync(string table, IHtValue hashKey, IHtValue rangeKey);
        bool ConditionalDeleteValue(string table, IHtValue hashKey, IHtValue rangeKey, ICheckCondition toCheck);
        Task<bool> ConditionalDeleteValueAsync(string table, IHtValue hashKey, IHtValue rangeKey, ICheckCondition toCheck);
        IEnumerable<IHtValue> Query(string table, IHtValue hashKey, IRangeCondition rangeCondition, bool requireConsistent = false);
        IEnumerable<IHtValue> Query(string table, IHtValue hashKey, IRangeCondition rangeCondition, CancellationToken cancellation, bool requireConsistent = false);
        IAsyncBatcher<IHtValue> QueryAsync(string table, IHtValue hashKey, IRangeCondition rangeCondition, bool requireConsistent = false);

        IAsyncBatcher<IHtValue> QueryAsync(string table, IHtValue hashKey, IRangeCondition rangeCondition,
            CancellationToken cancellation, bool requireConsistent = false);

        IEnumerable<IHtValue> Scan(string table, IScanCondition scanCondition);
        IEnumerable<IHtValue> Scan(string table, IScanCondition scanCondition, CancellationToken cancellation);

        IAsyncBatcher<IHtValue> ScanAsync(string table, IScanCondition scanCondition);
        IAsyncBatcher<IHtValue> ScanAsync(string table, IScanCondition scanCondition, CancellationToken cancellation);

        IEnumerable<long> Count(string table, IScanCondition condition);
        IEnumerable<long> Count(string table, IScanCondition condition, CancellationToken cancellation);
        Task<long> CountAsync(string table, IScanCondition condition);
        Task<long> CountAsync(string table, IScanCondition condition, CancellationToken cancellation);

        Task BatchPutAsync(string table, string hashKeyName, string rangeKeyName, IEnumerable<IHtValue> rows, bool replace=true);
        Task BatchPutAsync(string table, string hashKeyName, string rangeKeyName, IEnumerable<IHtValue> rows, CancellationToken cancellation, bool replace=true);
        void BatchPut(string table, string hashKeyName, string rangeKeyName, IEnumerable<IHtValue> rows, bool replace=true);
        void BatchPut(string table, string hashKeyName, string rangeKeyName, IEnumerable<IHtValue> rows, CancellationToken cancellation, bool replace=true);
        
        Task<IHtValue> GetTablePropertiesAsync(string table);
        IHtValue GetTableProperties(string table);
        List<string> GetTableNameList();
        Task<List<string>> GetTableNameListAsync();

        void DeleteTable(string table);
        Task DeleteTableAsync(string table);

        IEnumerable<IHtValue> Query(string table, IMultiKeyCondition keyCondition, bool requireConsistent = false);
        IEnumerable<IHtValue> Query(string table, IMultiKeyCondition keyCondition, CancellationToken cancellation, bool requireConsistent = false);
        IAsyncBatcher<IHtValue> QueryAsync(string table, IMultiKeyCondition keyCondition, bool requireConsistent = false);

        IAsyncBatcher<IHtValue> QueryAsync(string table, IMultiKeyCondition keyCondition,
            CancellationToken cancellation, bool requireConsistent = false);    
    }

    public static class NoSqlConstants
    {
        public const string HashKeyName = "HashKeyName";
        public const string RangeKeyName = "RangeKeyName";
    }
}
