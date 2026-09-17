using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminGetUserById;

public class AdminGetUserByIdQueryHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();

    private AdminGetUserByIdQueryHandler CreateHandler() => new(_userRepository);

    [Fact]
    public async Task Handle_WithExistingUser_ReturnsDetail()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), "Test", "User", null, UserRole.Candidate);
        _userRepository.Add(user);

        var result = await CreateHandler().Handle(new AdminGetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal("aday@example.com", result.Value.Email);
        Assert.Equal("Candidate", result.Value.Role);
        Assert.Equal("Active", result.Value.Status);
        Assert.False(result.Value.EmailConfirmed);
        Assert.False(result.Value.IsLockedOut);
        Assert.Equal(0, result.Value.FailedLoginAttemptCount);
    }

    [Fact]
    public async Task Handle_WithLockedUser_ReturnsLockoutDetails()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), "Test", "User", null, UserRole.Candidate);
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        _userRepository.Add(user);

        var result = await CreateHandler().Handle(new AdminGetUserByIdQuery(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.IsLockedOut);
        Assert.Equal(5, result.Value.FailedLoginAttemptCount);
        Assert.NotNull(result.Value.LockedUntilUtc);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new AdminGetUserByIdQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
    }
}
