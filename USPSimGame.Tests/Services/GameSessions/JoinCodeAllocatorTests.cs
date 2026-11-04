using System.Diagnostics;
using USPSimGame.Exceptions;
using USPSimGame.Services.GameSessions;

namespace USPSimGame.Tests.Services.GameSessions;

public class JoinCodeAllocatorTests
{
    public const int AmountOfCodes = 10000;

    [Fact]
    public async Task AllocateAsync_10000Codes_AreAllUnique()
    {
        //Arrange
        var allocator = new JoinCodeAllocator(new JoinCodeGenerator());
        var store = new HashSet<string>();

        //Act
        for (int i = 0; i < 10000; i++)
        {
            await allocator.AllocateAsync(code => Task.FromResult(store.Add(code)));
        }

        //Assert
        Assert.Equal(AmountOfCodes, store.Count);
    }

    [Fact]
    public async Task AllocateAsync_AlwaysCollides_ThrowsAfterMaxAttempts()
    {
        //Arrange
        var allocator = new JoinCodeAllocator(new JoinCodeGenerator());
        var attempts = 0;

        //Act & Assert
        var exception = await Assert.ThrowsAsync<JoinCodeAllocationException>(
            () => allocator.AllocateAsync(code =>
            {
                attempts++;
                return Task.FromResult(false);
            }));

        Assert.Equal(JoinCodeAllocator.MaxAttempts, attempts);
        Assert.Equal(JoinCodeAllocator.MaxAttempts, exception.Attempts);
    }

    [Fact]
    public async Task AllocateAsync_CodeAlreadyTaken_RetriesAndReturnsNewCode()
    {
        //Arrange
        var generator = new SequenceGenerator("111111", "111111", "222222");
        var allocator = new JoinCodeAllocator(generator);
        var store = new HashSet<string> { "111111" };

        //Act
        var code = await allocator.AllocateAsync(c => Task.FromResult(store.Add(c)));

        //Assert
        Assert.Equal("222222", code);
        Assert.Equal(3, generator.Calls);
    }

    [Fact]
    public async Task AllocateAsync_SingleCode_TakesUnderOneSecond()
    {
        //Arrange
        var allocator = new JoinCodeAllocator(new JoinCodeGenerator());
        var store = new HashSet<string>();

        //Act
        var stopwatch = Stopwatch.StartNew();
        await allocator.AllocateAsync(code => Task.FromResult(store.Add(code)));
        stopwatch.Stop();

        //Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 1000,
            $"Allocating a code took {stopwatch.ElapsedMilliseconds} ms.");
    }

    // Returns the given codes in order, and repeats the last one when it runs out.
    private sealed class SequenceGenerator(params string[] codes) : IJoinCodeGenerator
    {
        private int _next;
        public int Calls => _next;

        public string Generate() => codes[Math.Min(_next++, codes.Length - 1)];
    }
}