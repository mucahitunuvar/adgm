using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.Logout;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.Logout;

public class LogoutCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeRefreshTokenGenerator _refreshTokenGenerator = new();
    private readonly FakeCurrentUserService _currentUserService = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private LogoutCommandHandler CreateHandler() =>
        new(_userRepository, _refreshTokenGenerator, _currentUserService, _unitOfWork);

    private (User User, string PlainToken) AddUserWithRefreshToken()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue("hash"),
            UserRole.Candidate);

        const string plainToken = "known-plain-token";
        user.IssueRefreshToken(_refreshTokenGenerator.Hash(plainToken), DateTime.UtcNow.AddDays(1));

        _userRepository.Add(user);
        return (user, plainToken);
    }

    [Fact]
    public async Task Handle_WithOwnToken_RevokesTokenAndReturnsSuccess()
    {
        var (user, plainToken) = AddUserWithRefreshToken();
        _currentUserService.UserId = user.Id;

        var result = await CreateHandler().Handle(new LogoutCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var token = user.FindRefreshToken(_refreshTokenGenerator.Hash(plainToken))!;
        Assert.True(token.IsRevoked);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsSuccessIdempotently()
    {
        _currentUserService.UserId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new LogoutCommand("never-issued"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithTokenBelongingToDifferentUser_ReturnsForbidden()
    {
        var (_, plainToken) = AddUserWithRefreshToken();
        _currentUserService.UserId = Guid.NewGuid();

        var result = await CreateHandler().Handle(new LogoutCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
        Assert.Equal("Auth.NotYourSession", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
