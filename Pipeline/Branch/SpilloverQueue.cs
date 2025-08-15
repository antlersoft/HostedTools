using System;
using System.Collections.Generic;
using System.IO;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Interface;

using TempHolder = System.Tuple<System.IDisposable, System.Collections.Generic.IEnumerator<com.antlersoft.HostedTools.Interface.IHtValue>>;

namespace com.antlersoft.HostedTools.Pipeline.Branch
{
    /// <summary>
    /// A queue for strings that spills to disk when exceeding a threshold.
    /// </summary>
    public class SpilloverQueue : IDisposable
    {
        private const int MaxInMemory = 200_000;
        private readonly Queue<IHtValue> _memoryQueue = new Queue<IHtValue>();
        private readonly Queue<TempHolder> _tempFiles = new Queue<TempHolder>();
        private readonly IPluginManager _pluginManager;
        private readonly IWorkMonitor _monitor;
        private IEnumerator<IHtValue> _currentFileReader;
        private int _count = 0;
        private TempFileTransform _tft;

        public SpilloverQueue(IPluginManager pluginManager, IWorkMonitor monitor)
        {
            _pluginManager = pluginManager;
            _monitor = monitor;
        }

        public int Count => _count;

        public void Enqueue(IHtValue item)
        {
            _memoryQueue.Enqueue(item);
            _count++;
            if (_memoryQueue.Count > MaxInMemory)
            {
                SpillToTempFile();
            }
        }

        public IHtValue Dequeue()
        {
            // If there are temp files, read from them first
            while (_tempFiles.Count > 0)
            {
                if (_currentFileReader == null)
                {
                    var tempFile = _tempFiles.Peek();
                    _currentFileReader = tempFile.Item2;
                }
                if (_currentFileReader.MoveNext())
                {
                    --_count;
                    return _currentFileReader.Current;
                }
                // End of file, clean up
                var finishedFile = _tempFiles.Dequeue();
                finishedFile.Item1.Dispose();
                _currentFileReader = null;
            }
            // If no temp files, read from memory
            if (_memoryQueue.Count > 0)
            {
                _count--;
                return _memoryQueue.Dequeue();
            }
            throw new InvalidOperationException("Queue is empty.");
        }

        private void SpillToTempFile()
        {
            if (_tft == null)
            {
                _tft = _pluginManager[typeof(TempFileTransform).FullName] as TempFileTransform;
            }
            var transform = _tft.GetTempFileTransform();
            var enumerator = transform.GetTransformed(_memoryQueue, _monitor).GetEnumerator();
            _tempFiles.Enqueue(new TempHolder(transform.Cast<IDisposable>(), enumerator));
        }

        public void Dispose()
        {
            while (_tempFiles.Count > 0)
            {
                var tempFile = _tempFiles.Dequeue();
                tempFile.Item1.Dispose();
            }
        }
    }
}