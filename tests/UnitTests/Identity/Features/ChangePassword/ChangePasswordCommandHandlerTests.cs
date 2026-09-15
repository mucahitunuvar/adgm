using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ChangePassword;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeCurrentUserService _currentUserService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ChangePasswordCommandHandler CreateHandler() =>
        new(_currentUserService, _userRepository, _passwordHasher, _unitOfWork);

    private User AddUser(string password)
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue(_passwordHasher.Hash(password)),
            UserRole.Candidate);
        _userRepository.Add(user);
        _currentUserService.UserId = user.Id;
        return user;
    }

    [Fact]
    public async Task Handle_WithCorrectCurrentPassword_UpdatesHashAndRevokesRefreshTokens()
    {
        var user = AddUser("OldPass123");
        var activeToken = user.IssueRefreshToken("token-hash", DateTime.UtcNow.AddDays(7));

        var result = await CreateHandler().Handle(
            new ChangePasswordCommand("OldPass123", "NewPass456"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(_passwordHasher.Verify("NewPass456", user.PasswordHash.Value));
        Assert.True(activeToken.IsRevoked);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithWrongCurrentPassword_ReturnsUnauthorized()
    {
        AddUser("OldPass123");

        var result = await CreateHandler().Handle(
            new ChangePasswordCommand("WrongPass", "NewPass456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithNoAuthenticatedUser_ReturnsUnauthorized()
    {
        var result = await CreateHandler().Handle(
            new ChangePasswordCommand("OldPass123", "NewPass456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.NotAuthenticated", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithAuthenticatedUserMissingFromRepository_ReturnsNotFound()
    {
        _currentUserService.UserId = Guid.NewGuid();

        var result = await CreateHandler().Handle(
            new ChangePasswordCommand("OldPass123", "NewPass456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("User.NotFound", result.Error.Code);
    }
}
