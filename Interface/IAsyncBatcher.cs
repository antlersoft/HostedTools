using System;
using System.Collections.Generic;

using System.Threading.Tasks;

namespace com.antlersoft.HostedTools.Interface
{
    public interface IAsyncBatcher<T>
    {
        Task<IEnumerable<T>> NextBatch();
    }
/*
    public static class AsyncBatcherHelper
    {
        public static async Task ForEach<T>(this IAsyncBatcher<T> batcher, Action<T> action)
        {
            for (IEnumerable<T> batch = await batcher.NextBatch().ConfigureAwait(false); batch != null; batch = await batcher.NextBatch().ConfigureAwait(false))
            {
                foreach (T v in batch)
                {
                    action.Invoke(v);
                }
            }
        }
        public static async Task ForEachAsync<T>(this IAsyncBatcher<T> batcher, Action<T> action, TaskFactory tf)
        {
            for (IEnumerable<T> batch = await batcher.NextBatch().ConfigureAwait(false); batch != null; batch = await batcher.NextBatch().ConfigureAwait(false))
            {
                foreach (T v in batch)
                {
                    await tf.FromAsync(action.BeginInvoke, action.EndInvoke, v, v).ConfigureAwait(false);
                }
            }
        }
    }
    */
}
