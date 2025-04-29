using RequestThrottler.Core;
using RequestThrottler.Services;
using System;
using System.Threading;

public class DefaultRequestThrottler : IRequestThrottler
{
    private readonly IInMemoryThrottleQueue _queue;

    public DefaultRequestThrottler(IInMemoryThrottleQueue queue)
    {
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
    }

    public async Task ExecuteAsync(Func<Task> action, TimeSpan timeout)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (timeout == TimeSpan.Zero) throw new ArgumentException("Timeout cannot be zero.", nameof(timeout));

        var cts = new CancellationTokenSource();
        var timeoutTask = Task.Delay(timeout, cts.Token);

        try
        {
            var actionTask = Task.Run(async () =>
            {
                await _queue.EnqueueAsync(async () =>
                {
                    await action();
                });
            });

            var completedTask = await Task.WhenAny(actionTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("The action did not complete within the allotted time.");
            }

            cts.Cancel(); // Cancel timeout if action completes
            await actionTask; // Await the action to propagate exceptions
        }
        finally
        {
            cts.Dispose();
        }
    }

    public async Task<TResult> ExecuteAsync<TResult>(Func<Task<TResult>> action, TimeSpan timeout)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));
        if (timeout == TimeSpan.Zero) throw new ArgumentException("Timeout cannot be zero.", nameof(timeout));

        var cts = new CancellationTokenSource();
        var timeoutTask = Task.Delay(timeout, cts.Token);

        try
        {
            var actionTask = Task.Run(async () =>
            {
                return await _queue.EnqueueAsync(async () =>
                {
                    return await action();
                });
            });

            var completedTask = await Task.WhenAny(actionTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                throw new TimeoutException("The action did not complete within the allotted time.");
            }

            cts.Cancel(); // Cancel timeout if action completes
            return await actionTask; // Await the action to propagate exceptions
        }
        finally
        {
            cts.Dispose();
        }
    }
}
