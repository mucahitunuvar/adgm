using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

public sealed class FakeIdentityService : IIdentityService
{
    public Result<Guid> CreateUserResult { get; set; } = Result.Success(Guid.NewGuid());

    public Result<Guid> CreateStaffUserResult { get; set; } = Result.Success(Guid.NewGuid());

    public IdentityUserProfile? UserProfileResult { get; set; }

    // Görev 8: çoklu-alıcı (bulk) senaryolarda her userId için farklı bir profil gerekir - tekil
    // UserProfileResult (tüm çağrılara aynı sabit değeri döner) bunun için yetersiz kalır. Burada
    // bulunamayan bir userId, geriye dönük uyumluluk için UserProfileResult'a düşer.
    public Dictionary<Guid, IdentityUserProfile> UserProfilesById { get; } = [];

    public bool DeactivateUserAsyncCalled { get; private set; }

    public Guid? DeactivatedUserId { get; private set; }

    public Task<IdentityUserProfile?> GetUserProfileAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(UserProfilesById.TryGetValue(userId, out var profile) ? profile : UserProfileResult);

    public Task<Result<Guid>> CreateUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        string role,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateUserResult);

    public Task<Result<Guid>> CreateStaffUserAsync(
        string email,
        string password,
        string firstName,
        string lastName,
        string? phoneNumber,
        string role,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateStaffUserResult);

    public Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        DeactivateUserAsyncCalled = true;
        DeactivatedUserId = userId;
        return Task.CompletedTask;
    }
}
