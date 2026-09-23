using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.UnitTests.Support.TestDoubles;

public sealed class FakeIdentityService : IIdentityService
{
    public Result<Guid> CreateUserResult { get; set; } = Result.Success(Guid.NewGuid());

    public Result<Guid> CreateStaffUserResult { get; set; } = Result.Success(Guid.NewGuid());

    public IdentityUserProfile? UserProfileResult { get; set; }

    // Görev senaryolarında birden çok farklı UserId için farklı profiller gerekir (açan, atanan,
    // yeni atanan) - tekil UserProfileResult tüm çağrılara aynı sabit değeri döneceği için yetersiz
    // kalır. Bulunamayan bir userId, geriye dönük uyumluluk için UserProfileResult'a düşer.
    public Dictionary<Guid, IdentityUserProfile> UserProfilesById { get; } = [];

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

    public Task DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
