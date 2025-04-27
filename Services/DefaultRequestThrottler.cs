using RequestThrottler.Core;
using RequestThrottler.Services;
using System;
using System.Threading.Tasks;

namespace RequestThrottler.Services
{
    public class DefaultRequestThrottler : IRequestThrottler
    {
        private readonly IInMemoryThrottleQueue _queue;

        public DefaultRequestThrottler(IInMemoryThrottleQueue queue)
        {
            _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        }

        public Task ExecuteAsync(Func<Task> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            // TCS ile, action tamamlanana dek bekleyeceğiz
            var tcs = new TaskCompletionSource();
            _queue.EnqueueAsync(async () =>
            {
                try
                {
                    await action();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            var tcs = new TaskCompletionSource<TResult>();
            _queue.EnqueueAsync(async () =>
            {
                try
                {
                    var res = await action();
                    tcs.SetResult(res);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }
    }
}
