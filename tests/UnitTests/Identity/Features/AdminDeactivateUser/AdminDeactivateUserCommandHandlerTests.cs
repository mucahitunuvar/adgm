using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminDeactivateUser;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminDeactivateUser;

public class AdminDeactivateUserCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCurrentUserService _currentUserService = new() { UserId = Guid.NewGuid() };
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AdminDeactivateUserCommandHandler CreateHandler() =>
        new(_userRepository, _currentUserService, NullLogger<AdminDeactivateUserCommandHandler>.Instance, _unitOfWork);

    [Fact]
    public async Task Handle_WithActiveUser_DeactivatesAndSavesChanges()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), UserRole.Candidate);
        _userRepository.Add(user);

        var result = await CreateHandler().Handle(new AdminDeactivateUserCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Disabled, user.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new AdminDeactivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
