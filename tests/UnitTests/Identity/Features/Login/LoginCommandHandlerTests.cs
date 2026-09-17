using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.Login;

public class LoginCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();
    private readonly FakeRefreshTokenGenerator _refreshTokenGenerator = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private LoginCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _jwtTokenGenerator, _refreshTokenGenerator, _unitOfWork);

    private User AddUser(string email, string password, UserRole role = UserRole.Candidate)
    {
        var user = User.Register(
            Email.Create(email).Value,
            PasswordHash.FromHashedValue(_passwordHasher.Hash(password)), "Test", "User", null,
            role);
        _userRepository.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokensAndIssuesRefreshToken()
    {
        var user = AddUser("aday@example.com", "Sifre123");

        var result = await CreateHandler().Handle(
            new LoginCommand("aday@example.com", "Sifre123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"access-token-for-{user.Id}", result.Value.AccessToken);
        Assert.NotEmpty(result.Value.RefreshToken);
        Assert.Single(user.RefreshTokens);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ReturnsGenericInvalidCredentials()
    {
        var result = await CreateHandler().Handle(
            new LoginCommand("unknown@example.com", "Sifre123"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsSameGenericErrorAsUnknownEmail()
    {
        AddUser("aday@example.com", "Sifre123");

        var result = await CreateHandler().Handle(
            new LoginCommand("aday@example.com", "WrongPassword1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidCredentials", result.Error.Code);
    }

    [Fact]
    public async Task Handle_AfterMaxFailedAttempts_LocksAccountAndReturnsAccountLocked()
    {
        AddUser("aday@example.com", "Sifre123");
        var handler = CreateHandler();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await handler.Handle(new LoginCommand("aday@example.com", "WrongPassword1"), CancellationToken.None);
        }

        var result = await handler.Handle(
            new LoginCommand("aday@example.com", "Sifre123"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("Auth.AccountLocked", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithDeactivatedAccount_ReturnsAccountDeactivated()
    {
        var user = AddUser("aday@example.com", "Sifre123");
        user.Deactivate();

        var result = await CreateHandler().Handle(
            new LoginCommand("aday@example.com", "Sifre123"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("Auth.AccountDeactivated", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithFewerThanMaxFailedAttempts_ThenCorrectPassword_SucceedsAndResetsCounter()
    {
        var user = AddUser("aday@example.com", "Sifre123");
        var handler = CreateHandler();

        await handler.Handle(new LoginCommand("aday@example.com", "WrongPassword1"), CancellationToken.None);
        await handler.Handle(new LoginCommand("aday@example.com", "WrongPassword1"), CancellationToken.None);

        var result = await handler.Handle(
            new LoginCommand("aday@example.com", "Sifre123"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, user.FailedLoginAttemptCount);
        Assert.Null(user.LockedUntilUtc);
    }
}
