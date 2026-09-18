using GenclikMerkezi.SharedKernel.Abstractions;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Exception? ThrowOnSave { get; set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (ThrowOnSave is not null)
        {
            throw ThrowOnSave;
        }

        SaveChangesCallCount++;
        return Task.FromResult(1);
    }
}
