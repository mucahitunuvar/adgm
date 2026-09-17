using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.GetCurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCurrentUserService _currentUserService = new();

    private GetCurrentUserQueryHandler CreateHandler() => new(_currentUserService, _userRepository);

    [Fact]
    public async Task Handle_WithAuthenticatedExistingUser_ReturnsUserDetails()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue("hash"), "Test", "User", null,
            UserRole.Candidate);
        _userRepository.Add(user);
        _currentUserService.UserId = user.Id;

        var result = await CreateHandler().Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal("aday@example.com", result.Value.Email);
        Assert.Equal("Candidate", result.Value.Role);
        Assert.Equal("Active", result.Value.Status);
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_ReturnsUnauthorized()
    {
        var result = await CreateHandler().Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.NotAuthenticated", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUserMissingFromRepository_ReturnsNotFound()
    {
        _currentUserService.UserId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("User.NotFound", result.Error.Code);
    }
}
