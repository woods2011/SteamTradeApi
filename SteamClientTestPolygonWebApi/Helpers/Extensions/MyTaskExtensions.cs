namespace SteamClientTestPolygonWebApi.Helpers.Extensions;

public static class MyTaskExtensions
{
    public static Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> values) => Task.WhenAll(values);
    
    public static async Task<Task<TResult>?> WhenFirstSuccessOrDefault<TResult>(this IEnumerable<Task<TResult>> tasks)
    {
        var tasksList = tasks.ToList();

        Task<TResult> completedTask;
        do
        {
            completedTask = await Task.WhenAny(tasksList);
            tasksList.Remove(completedTask);
        } while (!completedTask.IsCompletedSuccessfully && tasksList.Count > 0);

        return completedTask.IsCompletedSuccessfully ? completedTask : null;
    }
    
    public static async Task<TResult> WhenFirstSuccess<TResult>(this IEnumerable<Task<TResult>> tasks)
    {
        var initialTaskList = tasks.ToList();
        if (initialTaskList.Count < 1)
            throw new ArgumentOutOfRangeException(nameof(tasks), "Number of elements should be greater than zero");

        var firstCompletedTaskOrDefault = await WhenFirstSuccessOrDefault(initialTaskList);
        return firstCompletedTaskOrDefault is not null
            ? await firstCompletedTaskOrDefault
            : await initialTaskList.Last();
    }
    
    public static async Task<Task<TResult>?> WhenFirstSuccessOrDefaultCancelOther<TResult>(
        Func<CancellationToken, Task<TResult>> taskFactory,
        int repeatTimes,
        CancellationToken token = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        var linkedToken = linkedTokenSource.Token;

        var tasksList = Enumerable.Range(0, repeatTimes).Select(_ => taskFactory(linkedToken)).ToList();
        var firstCompletedTaskOrDefault = await tasksList.WhenFirstSuccessOrDefault();

        linkedTokenSource.Cancel();
        return firstCompletedTaskOrDefault;
    }

    public static async Task<TResult> WhenFirstSuccessCancelOther<TResult>(
        Func<CancellationToken, Task<TResult>> taskFactory,
        int repeatTimes,
        CancellationToken token = default)
    {
        var linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
        var linkedToken = linkedTokenSource.Token;

        var tasksList = Enumerable.Range(0, repeatTimes).Select(_ => taskFactory(linkedToken)).ToList();
        var firstCompletedTask = await tasksList.WhenFirstSuccess();

        linkedTokenSource.Cancel();
        return firstCompletedTask;
    }
}