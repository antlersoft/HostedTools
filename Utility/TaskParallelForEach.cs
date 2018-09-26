using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace En.Ods.Lib.Common.Utility
{
    /// <summary>
    /// Runs a set of tasks determined by the value of an enumeration as parallel tasks (on the ThreadPool).
    /// No more than a set number will be active at a time.
    /// <para></para>
    /// The implementing class provides methods to return the task to run, and a method to handle the result of the task.
    /// These two methods are always called within a single parent task, so the implementing class has less
    /// task coordination to worry about. 
    /// </summary>
    /// <typeparam name="S">Type of item returned by enumeration</typeparam>
    /// <typeparam name="T">Type of result of task</typeparam>
    public abstract class TaskParallelForEachBase<S, T>
    {
        private int _maxParallel;
        private List<Task<T>> _tasks;

        public TaskParallelForEachBase(int maxParallel)
        {
            if (maxParallel < 1)
            {
                maxParallel = 1;
            }
            _maxParallel = maxParallel;
        }

        protected abstract Task<T> GetNextTask(S item);

        protected abstract void HandleResult(T result);

        public int RunningTasks
        {
            get { return _tasks == null ? 0 : _tasks.Count; }
        }

        public IList<Task<T>> TaskCollection
        {
            get { return _tasks; }
        }

        public async Task Run(IEnumerable<S> items, CancellationToken cancellation)
        {
            _tasks = new List<Task<T>>();
            foreach (S s in items)
            {
                if (cancellation.IsCancellationRequested)
                {
                    foreach (var task in _tasks)
                    {
                        if (task.IsCompleted)
                        {
                            HandleResult(task.Result);
                        }
                    }
                    return;
                }
                if (_tasks.Count >= _maxParallel)
                {
                    await Task.WhenAny(_tasks).ConfigureAwait(false);
                    List<Task<T>> newTaskList = new List<Task<T>>();
                    foreach (var task in _tasks)
                    {
                        if (task.IsCompleted)
                        {
                            HandleResult(task.Result);
                        }
                        else
                        {
                            newTaskList.Add(task);
                        }
                    }
                    _tasks = newTaskList;
                }
                var nextTask = GetNextTask(s);
                if (nextTask == null)
                {
                    continue;
                }
                _tasks.Add(nextTask);
            }
            await Task.WhenAll(_tasks).ConfigureAwait(false);
            foreach (var task in _tasks)
            {
                if (task.IsCompleted)
                {
                    HandleResult(task.Result);
                }
            }
        }

        public Task Run(IEnumerable<S> items)
        {
            return Run(items, default(CancellationToken));
        }
    }

    public class TaskParallelForEach<S, T> : TaskParallelForEachBase<S, T>
    {
        private Func<S, Task<T>> _getTask;
        private Action<T> _handleResult;

        public TaskParallelForEach(int numberParallel, Func<S, Task<T>> getTask, Action<T> handleResult)
            : base(numberParallel)
        {
            _getTask = getTask;
            _handleResult = handleResult;
        }

        protected override Task<T> GetNextTask(S item)
        {
            return _getTask(item);
        }

        protected override void HandleResult(T result)
        {
            _handleResult(result);
        }
    }
}