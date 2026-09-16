using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.RefreshAccessToken;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.RefreshAccessToken;

public class RefreshAccessTokenCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeJwtTokenGenerator _jwtTokenGenerator = new();
    private readonly FakeRefreshTokenGenerator _refreshTokenGenerator = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RefreshAccessTokenCommandHandler CreateHandler() =>
        new(_userRepository, _jwtTokenGenerator, _refreshTokenGenerator, _unitOfWork);

    private (User User, string PlainToken) AddUserWithRefreshToken(
        DateTime expiresAtUtc,
        bool revoked = false,
        UserRole role = UserRole.Candidate)
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue("hash"),
            role);

        var plainToken = "known-plain-token";
        var hash = _refreshTokenGenerator.Hash(plainToken);
        var token = user.IssueRefreshToken(hash, expiresAtUtc);

        if (revoked)
        {
            token.Revoke();
        }

        _userRepository.Add(user);
        return (user, plainToken);
    }

    [Fact]
    public async Task Handle_WithValidActiveToken_RotatesTokenAndReturnsNewTokens()
    {
        var (user, plainToken) = AddUserWithRefreshToken(DateTime.UtcNow.AddDays(1));

        var result = await CreateHandler().Handle(
            new RefreshAccessTokenCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"access-token-for-{user.Id}", result.Value.AccessToken);
        Assert.NotEqual(plainToken, result.Value.RefreshToken);

        var oldToken = user.FindRefreshToken(_refreshTokenGenerator.Hash(plainToken))!;
        Assert.True(oldToken.IsRevoked);
        Assert.Equal(2, user.RefreshTokens.Count);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsInvalidRefreshToken()
    {
        var result = await CreateHandler().Handle(
            new RefreshAccessTokenCommand("never-issued-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithAlreadyRevokedToken_ReturnsReuseDetected_AndRevokesAllActiveTokens()
    {
        var (user, plainToken) = AddUserWithRefreshToken(DateTime.UtcNow.AddDays(1), revoked: true);
        var otherActiveToken = user.IssueRefreshToken("other-active-hash", DateTime.UtcNow.AddDays(1));

        var result = await CreateHandler().Handle(
            new RefreshAccessTokenCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.RefreshTokenReuseDetected", result.Error.Code);
        Assert.True(otherActiveToken.IsRevoked);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithExpiredButNotRevokedToken_ReturnsInvalidRefreshToken()
    {
        var (_, plainToken) = AddUserWithRefreshToken(DateTime.UtcNow.AddSeconds(-1));

        var result = await CreateHandler().Handle(
            new RefreshAccessTokenCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidRefreshToken", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ReturnsForbidden()
    {
        var (user, plainToken) = AddUserWithRefreshToken(DateTime.UtcNow.AddDays(1));

        for (var attempt = 0; attempt < 5; attempt++)
        {
            user.RegisterFailedLoginAttempt();
        }

        var result = await CreateHandler().Handle(
            new RefreshAccessTokenCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("Auth.AccountNotActive", result.Error.Code);
    }

    [Fact]
    public async Task Handle_WithDeactivatedAccount_ReturnsAccountDeactivated()
    {
        var (user, plainToken) = AddUserWithRefreshToken(DateTime.UtcNow.AddDays(1));
        user.Deactivate();

        var result = await CreateHandler().Handle(
            new RefreshAccessTokenCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("Auth.AccountDeactivated", result.Error.Code);
    }
}
