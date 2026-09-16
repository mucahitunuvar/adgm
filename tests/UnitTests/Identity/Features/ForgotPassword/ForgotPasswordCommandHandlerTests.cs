using GenclikMerkezi.Contracts.IntegrationEvents;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ForgotPassword;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordResetTokenGenerator _tokenGenerator = new();
    private readonly FakeIntegrationEventPublisher _integrationEventPublisher = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ForgotPasswordCommandHandler CreateHandler() =>
        new(_userRepository, _tokenGenerator, _integrationEventPublisher, _unitOfWork);

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
    public async Task Handle_WithExistingActiveUser_IssuesTokenAndPublishesIntegrationEvent()
    {
        var user = AddUser();

        var result = await CreateHandler().Handle(new ForgotPasswordCommand("aday@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.PasswordResetTokens);

        var (topic, integrationEvent) = Assert.Single(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(IntegrationEventTopics.PasswordResetRequested, topic);
        var passwordResetEvent = Assert.IsType<PasswordResetRequestedIntegrationEvent>(integrationEvent);
        Assert.Equal(user.Id, passwordResetEvent.UserId);
        Assert.Equal("aday@example.com", passwordResetEvent.Email);
        Assert.NotEmpty(passwordResetEvent.ResetToken);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_StillReturnsSuccess_ButDoesNotPublish()
    {
        var result = await CreateHandler().Handle(new ForgotPasswordCommand("unknown@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithLockedAccount_ReturnsSuccess_ButDoesNotIssueTokenOrPublish()
    {
        var user = AddUser("aday@example.com", UserStatus.Locked);

        var result = await CreateHandler().Handle(new ForgotPasswordCommand("aday@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(user.PasswordResetTokens);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
    }

    [Fact]
    public async Task Handle_WithInvalidEmailFormat_StillReturnsSuccess()
    {
        var result = await CreateHandler().Handle(new ForgotPasswordCommand("not-an-email"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
    }
}
