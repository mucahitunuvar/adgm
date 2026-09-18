using GenclikMerkezi.Contracts.IntegrationEvents;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.CreateStaffUser;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.CreateStaffUser;

public class CreateStaffUserCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeEmailVerificationTokenGenerator _verificationTokenGenerator = new();
    private readonly FakeIntegrationEventPublisher _integrationEventPublisher = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateStaffUserCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _verificationTokenGenerator, _integrationEventPublisher, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidInput_CreatesUserAndReturnsSuccess()
    {
        var command = new CreateStaffUserCommand(
            "danisman@example.com", "Sifre123", "Ayşe", "Kaya", "05551234567", "CareerAdvisor");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("danisman@example.com", result.Value.Email);
        Assert.Equal("CareerAdvisor", result.Value.Role);
        Assert.Single(_userRepository.Users);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithValidInput_IssuesVerificationTokenAndPublishesIntegrationEvent()
    {
        var command = new CreateStaffUserCommand(
            "danisman@example.com", "Sifre123", "Ayşe", "Kaya", "05551234567", "CareerAdvisor");

        await CreateHandler().Handle(command, CancellationToken.None);

        var user = _userRepository.Users.Single();
        Assert.False(user.EmailConfirmed);
        Assert.Single(user.EmailVerificationTokens);

        var (topic, integrationEvent) = Assert.Single(_integrationEventPublisher.PublishedEvents);
        Assert.Equal(IntegrationEventTopics.UserRegistered, topic);
        var userRegisteredEvent = Assert.IsType<UserRegisteredIntegrationEvent>(integrationEvent);
        Assert.Equal(user.Id, userRegisteredEvent.UserId);
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ReturnsConflict_AndDoesNotPersistAgain()
    {
        var existingUser = User.Register(
            Email.Create("danisman@example.com").Value,
            PasswordHash.FromHashedValue("hash"), "Test", "User", null,
            UserRole.CareerAdvisor);
        _userRepository.Add(existingUser);

        var command = new CreateStaffUserCommand(
            "danisman@example.com", "Sifre123", "Ayşe", "Kaya", null, "CareerAdvisor");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(_userRepository.Users);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.Empty(_integrationEventPublisher.PublishedEvents);
    }
}
