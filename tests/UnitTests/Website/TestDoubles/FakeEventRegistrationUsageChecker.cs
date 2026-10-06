using GenclikMerkezi.Modules.Website.Application.Abstractions;

namespace GenclikMerkezi.UnitTests.Website.TestDoubles;

public sealed class FakeEventRegistrationUsageChecker : IEventRegistrationUsageChecker
{
    public bool HasRegistrationsResult { get; set; }

    public Task<bool> HasRegistrationsAsync(Guid contentItemId, CancellationToken cancellationToken = default) =>
        Task.FromResult(HasRegistrationsResult);
}
