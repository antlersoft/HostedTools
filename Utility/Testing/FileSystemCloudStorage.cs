using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Utility.Testing
{
    /// <summary>
    /// Implementation of ICloudStorage that uses resources in the file system
    /// </summary>
    public class FileSystemCloudStorage : ICloudStorage
    {
        private Dictionary<string, string> _bucketMap = new Dictionary<string, string>();
        private readonly bool _ignoreWrites;
        private readonly object _mapLock = new object();

        /// <summary>
        /// Create an instance of ICloudStorage that uses resources in the file system
        /// <para>
        /// You can create a mapping between bucket names and paths in the file system,
        /// so keys within a bucket that is not a path map to actual paths.
        /// </para>
        /// </summary>
        public FileSystemCloudStorage()
            : this(false)
        { }

        /// <summary>
        /// Create an instance of ICloudStorage that uses resources in the file system, optionally ignoring any update
        /// calls through the interface
        /// </summary>
        /// <param name="ignoreWrites">If true, any calls that would create/modify/delete stored data are silently ignored</param>
        public FileSystemCloudStorage(bool ignoreWrites)
        {
            _ignoreWrites = ignoreWrites;
        }

        /// <summary>
        /// Create a mapping between a bucket name and a path in the file system
        /// </summary>
        /// <param name="bucket">Bucket name</param>
        /// <param name="path">Path in file system corresponding to bucket</param>
        public void SetBucketMap(string bucket, string path)
        {
            lock (_mapLock)
            {
                _bucketMap[bucket] = path;
            }
        }

        /// <summary>
        /// Remove a mapping between a bucket name and a path in the file system
        /// </summary>
        /// <param name="bucket">Name of bucket with mapping to remove</param>
        /// <returns>True if the mapping existed and was removed; false if no mapping for the bucket existed</returns>
        public bool RemoveBucketMap(string bucket)
        {
            lock (_mapLock)
            {
                return _bucketMap.Remove(bucket);
            }
        }

        private string GetPath(string bucket, string key)
        {
            string mapped;
            lock (_mapLock)
            {
                if (_bucketMap == null)
                {
                    throw new ObjectDisposedException("FileSystemCloudStorage");
                }
                if (! _bucketMap.TryGetValue(bucket, out mapped))
                {
                    mapped = bucket;
                }
            }
            return Path.Combine(mapped, key);
        }

        private string GetPathCreatingFolder(string bucket, string key)
        {
            string path = GetPath(bucket, key);
            string folder = Path.GetDirectoryName(path);
            if (folder != null)
            {
                Directory.CreateDirectory(folder);
            }
            return path;
        }

        public void DeleteObject(string bucket, string key, IHtExpression condition = null)
        {
            if (_ignoreWrites)
            {
                return;
            }
            File.Delete(GetPath(bucket, key));
        }

        public Task DeleteObjectAsync(string bucket, string key, IHtExpression condition = null)
        {
            if (_ignoreWrites)
            {
                return Task.FromResult(true);
            }
            return Task.Run(() => File.Delete(GetPath(bucket, key)));
        }

        IEnumerable<ICloudStorageKey> RecursiveGetKeys(string folderPath, string bucket, string matchingPart, int matchingOffset)
        {
            string toMatch = matchingPart.Replace("/", "\\");
            foreach (string file in Directory.EnumerateFileSystemEntries(folderPath))
            {
                string subPath = Path.Combine(folderPath, file);
                if (subPath.Substring(matchingOffset).StartsWith(toMatch, StringComparison.OrdinalIgnoreCase))
                {
                    if (Directory.Exists(subPath))
                    {
                        foreach (var result in RecursiveGetKeys(subPath, bucket, matchingPart, matchingOffset))
                        {
                            yield return result;
                        }
                    }
                    else
                    {
                        yield return new FileSystemCloudStorageKey(bucket, subPath.Substring(matchingOffset),
                            File.GetLastWriteTimeUtc(subPath), new FileInfo(subPath).Length);
                    }
                }
            }
        }
        public IEnumerable<ICloudStorageKey> GetKeys(string bucket, string matchingPart, IHtExpression condition = null)
        {
            String path = GetPath(bucket, matchingPart);
            int matchingOffset = path.Length - matchingPart.Length;
            string directory = Directory.Exists(path) ? path : Path.GetDirectoryName(path);
            return RecursiveGetKeys(directory, bucket, matchingPart, matchingOffset);
        }

        class KeyBatcher : IAsyncBatcher<ICloudStorageKey>
        {
            private IEnumerable<ICloudStorageKey> _keys;

            internal KeyBatcher(IEnumerable<ICloudStorageKey> keys)
            {
                _keys = keys;
            }

            public Task<IEnumerable<ICloudStorageKey>> NextBatch()
            {
                var result = _keys;
                _keys = null;
                return Task.FromResult(result);
            }
        }

        public IAsyncBatcher<ICloudStorageKey> GetKeysAsync(string bucket, string matchingPart, CancellationToken cancellation, IHtExpression condition = null)
        {
            return new KeyBatcher(GetKeys(bucket, matchingPart));
        }

        public ICloudObject GetObject(string bucket, string key, IHtExpression condition = null)
        {
            string path = GetPath(bucket, key);
            return new FileSystemCloudObject(path, new FileSystemCloudStorageKey(bucket, key, File.GetLastWriteTimeUtc(path), new FileInfo(path).Length));
        }

        public Task<ICloudObject> GetObjectAsync(string bucket, string path, IHtExpression condition = null)
        {
            return Task.FromResult(GetObject(bucket, path));
        }

        public string GetPresignedUrl(string bucket, string key, int minutes)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetPresignedUrlAsync(string bucket, string key, int minutes)
        {
            throw new NotImplementedException();
        }

        public void PutObject(string bucket, string key, byte[] bytes, string mimeType = null)
        {
            if (_ignoreWrites) return;
            using (var fs = new FileStream(GetPathCreatingFolder(bucket, key), FileMode.Create))
            {
                fs.Write(bytes, 0, bytes.Length);
            }
        }

        public void PutObject(string bucket, string key, byte[] bytes, bool encrypt, string mimeType = null)
        {
            if (_ignoreWrites) return;
            throw new NotImplementedException();
        }

        public void PutObject(string bucket, string key, Stream bytes, string mimeType = null)
        {
            if (_ignoreWrites) return;
            using (var fs = new FileStream(GetPathCreatingFolder(bucket, key), FileMode.Create))
            {
                bytes.CopyTo(fs);
            }
        }

        public void PutObject(string bucket, string key, Stream bytes, bool encrypt, string mimeType = null)
        {
            if (_ignoreWrites) return;
            throw new NotImplementedException();
        }

        public async Task PutObjectAsync(string bucket, string key, byte[] bytes, string mimeType = null)
        {
            if (_ignoreWrites) return;
            using (var fs = new FileStream(GetPathCreatingFolder(bucket, key), FileMode.Create))
            {
                await fs.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);
            }
        }

        public Task PutObjectAsync(string bucket, string key, byte[] bytes, bool encrypt, string mimeType = null)
        {
            if (_ignoreWrites) return Task.FromResult(true);
            throw new NotImplementedException();
        }

        public async Task PutObjectAsync(string bucket, string key, Stream bytes, string mimeType = null)
        {
            if (_ignoreWrites) return;
            using (var fs = new FileStream(GetPathCreatingFolder(bucket, key), FileMode.Create))
            {
                await bytes.CopyToAsync(fs).ConfigureAwait(false);
            }
        }

        public Task PutObjectAsync(string bucket, string key, System.IO.Stream bytes, bool encrypt, string mimeType = null)
        {
            if (_ignoreWrites) return Task.FromResult(true);
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _bucketMap = null;
        }
    }

    internal class FileSystemCloudObject : ICloudObject
    {
        private readonly string _path;
        private readonly ICloudStorageKey _key;

        internal FileSystemCloudObject(string path, ICloudStorageKey key)
        {
            _path = path;
            _key = key;
        }
        public ICloudStorageKey Key
        {
            get { return _key; }
        }

        public Stream Stream
        {
            get { return new FileStream(_path, FileMode.Open); }
        }

        public void Dispose()
        {
        }
    }

    internal class FileSystemCloudStorageKey : ICloudStorageKey
    {
        internal FileSystemCloudStorageKey(string bucket, string path, DateTime lastModified, long length)
        {
            Bucket = bucket;
            Path = path;
            LastModified = lastModified;
            Size = length;
        }
        public string Bucket { get; private set; }

        public DateTime LastModified { get; private set; }

        public string Path { get; private set; }

        public long Size { get; private set; }
    }
}
