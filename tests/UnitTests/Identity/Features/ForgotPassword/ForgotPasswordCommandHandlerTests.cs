using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ForgotPassword;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordResetTokenGenerator _tokenGenerator = new();
    private readonly FakePasswordResetTokenNotifier _notifier = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ForgotPasswordCommandHandler CreateHandler() =>
        new(_userRepository, _tokenGenerator, _notifier, _unitOfWork);

    private User AddUser(string email = "aday@example.com") =>
        AddUser(email, UserStatus.Active);

    private User AddUser(string email, UserStatus status)
    {
        var user = User.Register(Email.Create(email).Value, PasswordHash.FromHashedValue("hash"), UserRole.Candidate);

        for (var i = 0; status == UserStatus.Locked && i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        _userRepository.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_WithExistingActiveUser_IssuesTokenAndNotifies()
    {
        var user = AddUser();

        var result = await CreateHandler().Handle(new ForgotPasswordCommand("aday@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.PasswordResetTokens);
        Assert.Equal(1, _notifier.NotifyCallCount);
        Assert.Equal("aday@example.com", _notifier.LastEmail);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_StillReturnsSuccess_ButDoesNotNotify()
    {
        var result = await CreateHandler().Handle(new ForgotPasswordCommand("unknown@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _notifier.NotifyCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ReturnsSuccess_ButDoesNotIssueTokenOrNotify()
    {
        var user = AddUser("aday@example.com", UserStatus.Locked);

        var result = await CreateHandler().Handle(new ForgotPasswordCommand("aday@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(user.PasswordResetTokens);
        Assert.Equal(0, _notifier.NotifyCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidEmailFormat_StillReturnsSuccess()
    {
        var result = await CreateHandler().Handle(new ForgotPasswordCommand("not-an-email"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, _notifier.NotifyCallCount);
    }
}
