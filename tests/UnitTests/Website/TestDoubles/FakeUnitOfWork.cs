using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    // Lets a test simulate a race (e.g. a unique-index violation) on the next SaveChangesAsync call
    // without depending on any concrete persistence exception type. OnSaveChangesFailure runs first,
    // so a test can use it to simulate the effect of a concurrent writer (e.g. seeding a competing row)
    // exactly at the point the real race would have landed.
    public Exception? FailNextSaveChangesWith { get; set; }

    public Action? OnSaveChangesFailure { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;

        if (FailNextSaveChangesWith is { } exception)
        {
            FailNextSaveChangesWith = null;
            OnSaveChangesFailure?.Invoke();
            throw exception;
        }

        return Task.FromResult(1);
    }
}
