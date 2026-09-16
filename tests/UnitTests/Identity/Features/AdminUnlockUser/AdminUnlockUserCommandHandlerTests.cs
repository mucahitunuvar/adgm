using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.AdminUnlockUser;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace GenclikMerkezi.UnitTests.Identity.Features.AdminUnlockUser;

public class AdminUnlockUserCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeCurrentUserService _currentUserService = new() { UserId = Guid.NewGuid() };
    private readonly FakeUnitOfWork _unitOfWork = new();

    private AdminUnlockUserCommandHandler CreateHandler() =>
        new(_userRepository, _currentUserService, NullLogger<AdminUnlockUserCommandHandler>.Instance, _unitOfWork);

    [Fact]
    public async Task Handle_WithLockedUser_UnlocksAndSavesChanges()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), UserRole.Candidate);
        for (var i = 0; i < 5; i++)
        {
            user.RegisterFailedLoginAttempt();
        }

        _userRepository.Add(user);

        var result = await CreateHandler().Handle(new AdminUnlockUserCommand(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.IsLockedOut);
        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(new AdminUnlockUserCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("User.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
