using FluentAssertions;
using SteamClientTestPolygonWebApi.Helpers.Extensions;

namespace SteamClientTestPolygonWebApi.UnitTests.TaskExtensionsTests;

public class Tests
{
    [Fact]
    public async Task WhenFirstSuccessOrDefault_ReturnsFirstSuccessfulTask_WhenAtLeastOneTaskSucceeds()
    {
        // Arrange
        Task<int>[] tasks = {
            Task.Delay(200).ContinueWith(t => 1),
            Task.FromResult(2),
            Task.FromException<int>(new Exception()),
            Task.Delay(300).ContinueWith(t => 1),
            Task.FromException<int>(new Exception())
        };
    
        // Act
        var result = await tasks.WhenFirstSuccessOrDefault();
    
        // Assert
        result.Should().NotBeNull();
        (await result!).Should().Be(2);
    }

    [Fact]
    public async Task WhenFirstSuccessOrDefault_ReturnsNull_WhenNoTaskSucceeds()
    {
        // Arrange
        Task<int>[] tasks = {
            Task.FromException<int>(new Exception()),
            Task.FromException<int>(new Exception())
        };

        // Act
        Task<int>? result = await tasks.WhenFirstSuccessOrDefault();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task WhenFirstSuccess_ReturnsFirstSuccessfulResult_WhenAtLeastOneTaskSucceeds()
    {
        // Arrange
        Task<int>[] tasks = {
            Task.Delay(200).ContinueWith(_ => 1),
            Task.FromResult(2),
            Task.FromException<int>(new Exception()),
            Task.Delay(300).ContinueWith(_ => 1),
            Task.FromException<int>(new Exception())
        };
    
        // Act
        var result = await tasks.WhenFirstSuccess();
    
        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async Task WhenFirstSuccess_ThrowsArgumentOutOfRangeException_WhenNoTasksAreProvided()
    {
        // Arrange
        Task<int>[] tasks = Array.Empty<Task<int>>();

        // Act
        var act = async () => await tasks.WhenFirstSuccess();
        
        // Act and Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task WhenFirstSuccessCancelOther_ReturnsFirstSuccessfulTask_WhenAtLeastOneTaskSucceeds()
    {
        // Arrange
        int counter = 0;
        Task<int> TaskFactory(CancellationToken token) => Task.FromResult(++counter);

        // Act
        var result = await MyTaskExtensions.WhenFirstSuccessCancelOther(TaskFactory, 5);

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public async Task WhenFirstSuccessCancelOther_ThrowsException_WhenNotTrulyAsyncTaskFactoryThrowsOneException()
    {
        // Arrange
        var counter = 0;
        Task<int> TaskFactory(CancellationToken token)
        {
            if (Interlocked.Increment(ref counter) == 3) throw new Exception("NotAsyncFactoryTestException");
            return Task.FromResult(5);
        }
        
        // Act
        var act = async () => await MyTaskExtensions.WhenFirstSuccessCancelOther(TaskFactory, 5);
        
        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("NotAsyncFactoryTestException");
    }
    
    [Fact]
    public async Task WhenFirstSuccessCancelOther_NotThrowsException_WhenAsyncTaskFactoryThrowsOneException()
    {
        // Arrange
        var counter = 0;
        async Task<int> TaskFactory(CancellationToken token)
        {
            if (Interlocked.Increment(ref counter) == 3) throw new Exception("AsyncFactoryTestException");
            return 5;
        }
        
        // Act
        var act = async () => await MyTaskExtensions.WhenFirstSuccessCancelOther(TaskFactory, 5);

        // Assert
        await act.Should().NotThrowAsync<Exception>();
    }
    
    [Fact]
    public async Task WhenFirstSuccessCancelOther_CancelsRemainingTasks_WhenFirstSuccessfulTaskIsFound()
    {
        // Arrange
        int taskNumber = 0;
        var resultCounter = 0;

        async Task<int> TaskFactory(CancellationToken token)
        {
            if (Interlocked.Increment(ref taskNumber) == 2) return ++resultCounter;
            await Task.Delay(300, token);
            return resultCounter++;
        }
        
        // Act
        var result = await MyTaskExtensions.WhenFirstSuccessCancelOther(TaskFactory, 10);
        await Task.Delay(1000);

        // Assert
        result.Should().Be(1);
    }
}