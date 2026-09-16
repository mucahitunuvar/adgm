using GenclikMerkezi.Contracts.IntegrationEvents;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ResendVerificationEmail;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ResendVerificationEmail;

public class ResendVerificationEmailCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeEmailVerificationTokenGenerator _tokenGenerator = new();
    private readonly FakeIntegrationEventPublisher _integrationEventPublisher = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ResendVerificationEmailCommandHandler CreateHandler() =>
        new(_userRepository, _tokenGenerator, _integrationEventPublisher, _unitOfWork);

    private User AddUnverifiedUser(string email = "aday@example.com")
    {
        var user = User.Register(Email.Create(email).Value, PasswordHash.FromHashedValue("hash"), UserRole.Candidate);
        _userRepository.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_WithUnverifiedUserAndNoPriorToken_IssuesTokenAndPublishesIntegrationEvent()
    {
        var user = AddUnverifiedUser();

        var result = await CreateHandler().Handle(
            new ResendVerificationEmailCommand("aday@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(user.EmailVerificationTokens);

        var (topic, integrationEvent) = Assert.Single(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(IntegrationEventTopics.EmailVerificationRequested, topic);
        var resendEvent = Assert.IsType<EmailVerificationRequestedIntegrationEvent>(integrationEvent);
        Assert.Equal(user.Id, resendEvent.UserId);
        Assert.Equal("aday@example.com", resendEvent.Email);
        Assert.NotEmpty(resendEvent.VerificationToken);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_CalledTwiceWithinOneMinute_OnlySendsOnce()
    {
        var user = AddUnverifiedUser();

        await CreateHandler().Handle(new ResendVerificationEmailCommand("aday@example.com"), CancellationToken.None);
        var secondResult = await CreateHandler().Handle(
            new ResendVerificationEmailCommand("aday@example.com"), CancellationToken.None);

        Assert.True(secondResult.IsSuccess);
        Assert.Single(user.EmailVerificationTokens);
        Assert.Single(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_StillReturnsSuccess_ButDoesNotPublish()
    {
        var result = await CreateHandler().Handle(
            new ResendVerificationEmailCommand("unknown@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyVerifiedUser_StillReturnsSuccess_ButDoesNotPublish()
    {
        var user = AddUnverifiedUser();
        var tokenHash = _tokenGenerator.Hash("already-issued-token");
        user.IssueEmailVerificationToken(tokenHash, DateTime.UtcNow.AddHours(48));
        user.ConfirmEmail(tokenHash);

        var result = await CreateHandler().Handle(
            new ResendVerificationEmailCommand("aday@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidEmailFormat_StillReturnsSuccess()
    {
        var result = await CreateHandler().Handle(
            new ResendVerificationEmailCommand("not-an-email"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
    }
}
