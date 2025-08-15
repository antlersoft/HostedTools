
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using com.antlersoft.HostedTools.Framework.Interface.Plugin;
using com.antlersoft.HostedTools.Framework.Interface.Setting;
using com.antlersoft.HostedTools.Framework.Model;
using com.antlersoft.HostedTools.Framework.Model.Menu;
using com.antlersoft.HostedTools.Framework.Model.Setting;
using com.antlersoft.HostedTools.Interface;

namespace com.antlersoft.HostedTools.Pipeline.Branch {

    [Export(typeof(IStemNode))]
    [Export(typeof(ISettingDefinitionSource))]
    public class Tee : AbstractPipelineNode, IHtValueStem, ISettingDefinitionSource {
        [Import]
        public IBranchManager BranchManager { get; set; }

        public override string NodeDescription => $"Write a copy to branch {BranchKey.Value<string>(SettingManager)}";

        public IEnumerable<ISettingDefinition> Definitions => new [] { BranchKey };

        public static ISettingDefinition BranchKey = new SimpleSettingDefinition("BranchKey", "Pipeline", "Branch Key", "Identifies the branch to match with other end");

        public Tee()
        : base(new MenuItem("DevTools.Pipeline.Transform.Tee", "Tee branch", typeof(Tee).FullName, "DevTools.Pipeline.Transform"),
            new[] { BranchKey.FullKey() })
        {

        }

        class Transform : HostedObjectBase, IHtValueTransform, IDisposable
        {
            private readonly IPluginManager _pluginManager;
            private readonly IBranchManager _branchManager;
            private readonly string _key;
            private readonly object _queueLock = new object();
            private readonly object _enumeratorLock = new object();

            private IWorkMonitor _monitor;
            private IBranchCollection _collection;
            private IBranchHtValueReceiver _receiver;
            private SpilloverQueue _mainQueue;
            private SpilloverQueue _branchQueue;
            private IEnumerator<IHtValue> _enumerator;
            bool _inputFinished = false;
            bool _disposed = false;
            internal Transform(IPluginManager pluginManager, IBranchManager branchManager, string key)
            {
                _branchManager = branchManager;
                _key = key;
            }

            public void Dispose()
            {
                lock (_enumeratorLock)
                {
                    lock (_queueLock)
                    {
                        _disposed = true;
                        _inputFinished = true;
                        if (_receiver != null)
                        {
                            _receiver.Finish();
                            _receiver = null;
                        }
                        if (_monitor != null)
                        {
                            _branchManager.FinishBranchCollection(_monitor, _key);
                            _monitor = null;
                            _collection = null;
                        }
                    }
                }
            }

            private IHtValue ReadFromSource(SpilloverQueue myQueue, SpilloverQueue otherQueue)
            {
                IHtValue resultValue = null;
                bool finished = false;
                lock (_queueLock)
                {
                    if (_disposed)
                    {
                        finished = true;
                    }
                    else if (myQueue.Count > 0)
                    {
                        resultValue = myQueue.Dequeue();
                        finished = true;
                    }
                    else if (_inputFinished)
                    {
                        finished = true;
                    }
                }
                bool currentGood = false;
                if (!finished)
                {
                    lock (_enumeratorLock)
                    {
                        if (! _disposed && _enumerator.MoveNext())
                        {
                            currentGood = true;
                            resultValue = _enumerator.Current;
                        }
                        lock (_queueLock)
                        {
                            if (_disposed)
                            {
                                resultValue = null;
                            }
                            else
                            {
                                if (!_inputFinished) _inputFinished = !currentGood;
                                if (myQueue.Count > 0)
                                {
                                    if (currentGood)
                                    {
                                        myQueue.Enqueue(resultValue);
                                        otherQueue.Enqueue(resultValue);
                                    }
                                    resultValue = myQueue.Dequeue();
                                }
                                else if (currentGood)
                                {
                                    otherQueue.Enqueue(resultValue);
                                }
                            }
                        }
                    }
                }
                return resultValue;
            }

            private IEnumerable<IHtValue> InternalGetTransformed()
            {
                for (IHtValue nextValue; (nextValue = ReadFromSource(_mainQueue, _branchQueue)) != null;)
                {
                    yield return nextValue;
                }
            }

            private IHtValue NextTeeValue()
            {
                var result = ReadFromSource(_branchQueue, _mainQueue);
                if (result == null)
                {
                    _receiver.Finish();
                }
                return result;
            }

            public IEnumerable<IHtValue> GetTransformed(IEnumerable<IHtValue> input, IWorkMonitor monitor)
            {
                _monitor = monitor;
                _collection = _branchManager.CreateBranchCollection(_monitor, _key);
                _receiver = _collection.GetNextReceiver(NextTeeValue);
                _mainQueue = new SpilloverQueue(_pluginManager, monitor);
                _branchQueue = new SpilloverQueue(_pluginManager, monitor);
                _enumerator = input.GetEnumerator();

                return InternalGetTransformed();
            }
        }

        public IHtValueTransform GetHtValueTransform(PluginState state)
        {
            return new Transform(PluginManager, BranchManager, state.SettingValues[BranchKey.FullKey()]);
        }
    }
}