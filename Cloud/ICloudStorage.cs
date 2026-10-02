using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Cloud
{
    public interface ICloudStorageKey
    {
        string Bucket { get; }
        string Path { get; }
        DateTime LastModified { get; }
    }

    public interface ICloudObject : IDisposable
    {
        ICloudStorageKey Key { get; }
        Stream Stream { get; }
    }

    public interface ICloudStorage : IDisposable
    {
        ICloudObject GetObject(string bucket, string path, IHtExpression? condition=null);
        Task<ICloudObject> GetObjectAsync(string bucket, string path, IHtExpression? condition=null);
        void DeleteObject(string bucket, string key, IHtExpression? condition=null);
        Task DeleteObjectAsync(string bucket, string key, IHtExpression? condition=null);
        IEnumerable<ICloudStorageKey> GetKeys(string bucket, string matchingPart, IHtExpression? condition = null);
        IAsyncBatcher<ICloudStorageKey> GetKeysAsync(string bucket, string matchingPart, CancellationToken cancellation, IHtExpression? condition = null);
        void PutObject(string bucket, string key, Stream bytes, bool encrypt, string? mimeType = null);
        void PutObject(string bucket, string key, Stream bytes, string? mimeType = null);
        Task PutObjectAsync(string bucket, string key, Stream bytes, bool encrypt, string? mimeType = null);
        Task PutObjectAsync(string bucket, string key, Stream bytes, string? mimeType = null);
        void PutObject(string bucket, string key, byte[] bytes, bool encrypt, string? mimeType = null);
        void PutObject(string bucket, string key, byte[] bytes, string? mimeType = null);
        Task PutObjectAsync(string bucket, string key, byte[] bytes, bool encrypt, string? mimeType = null);
        Task PutObjectAsync(string bucket, string key, byte[] bytes, string? mimeType = null);
        string GetPresignedUrl(string bucket, string key, int minutes);
        Task<string> GetPresignedUrlAsync(string bucket, string key, int minutes);
    }

    public static class CloudStorageConstants
    {
        public const string LastUpdatedTicks = "LastUpdatedUnixTime";
        public const string Name = "Name";
        public const string CompletePath = "CompletePath";
    }
}
