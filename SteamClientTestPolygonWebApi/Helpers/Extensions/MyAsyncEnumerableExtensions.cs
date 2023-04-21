namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class MyAsyncEnumerableExtensions
{
    public static IAsyncEnumerable<Task<TResult>> RunTasksWithDelay<TInput, TResult>(
        this IEnumerable<TInput> inputs,
        Func<TInput, Task<TResult>> taskSelector,
        double tasksPerSec,
        CancellationToken token = default)
    {
        if (tasksPerSec <= 1e-3) throw new ArgumentOutOfRangeException(nameof(tasksPerSec));

        var delayBetweenTasks = TimeSpan.FromSeconds(1) / tasksPerSec;

        return inputs.ToAsyncEnumerable().SelectAwait(async input =>
        {
            await Task.Delay(delayBetweenTasks, token);
            return taskSelector(input);
        });
    }

    public static IAsyncEnumerable<Task<TResult>> RunTasksWithDelay<TInput, TResult>(
        this IEnumerable<TInput> inputs,
        Func<TInput, Task<TResult>> taskSelector,
        double tasksPerSec,
        int maxSimultaneouslyRunningTasksCount,
        CancellationToken token = default)
    {
        if (tasksPerSec <= 1e-3) throw new ArgumentOutOfRangeException(nameof(tasksPerSec));
        if (maxSimultaneouslyRunningTasksCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSimultaneouslyRunningTasksCount));

        var delayBetweenTasks = TimeSpan.FromSeconds(1) / tasksPerSec;
        var semaphore = new SemaphoreSlim(maxSimultaneouslyRunningTasksCount);

        return inputs.ToAsyncEnumerable()
            .SelectAwait(async input =>
            {
                await Task.Delay(delayBetweenTasks, token);
                await semaphore.WaitAsync(token);

                Task<TResult> selector = taskSelector(input);
                _ = selector.ContinueWith(_ => semaphore.Release(), token);
                return selector;
            });
    }
}