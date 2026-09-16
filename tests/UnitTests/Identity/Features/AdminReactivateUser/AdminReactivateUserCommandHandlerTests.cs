using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminReactivateUser;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminReactivateUser;

public class AdminReactivateUserCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCurrentUserService _currentUserService = new() { UserId = Guid.NewGuid() };
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AdminReactivateUserCommandHandler CreateHandler() =>
        new(_userRepository, _currentUserService, NullLogger<AdminReactivateUserCommandHandler>.Instance, _unitOfWork);

    [Fact]
    public async Task Handle_WithDeactivatedUser_ReactivatesAndSavesChanges()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), UserRole.Candidate);
        user.Deactivate();
        _userRepository.Add(user);

        var result = await CreateHandler().Handle(new AdminReactivateUserCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new AdminReactivateUserCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
