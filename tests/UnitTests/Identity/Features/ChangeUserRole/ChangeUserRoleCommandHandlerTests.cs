using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ChangeUserRole;

public class ChangeUserRoleCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ChangeUserRoleCommandHandler CreateHandler() => new(_userRepository, _unitOfWork);

    private User AddUser(UserRole role = UserRole.Candidate)
    {
        var user = User.Register(Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), "Test", "User", null, role);
        _userRepository.Add(user);
        return user;
    }

    [Fact]
    public async Task Handle_WithExistingUser_ChangesRole()
    {
        var user = AddUser(UserRole.Candidate);

        var result = await CreateHandler().Handle(
            new ChangeUserRoleCommand(user.Id, nameof(UserRole.CareerAdvisor)), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.CareerAdvisor, user.Role);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownUserId_ReturnsNotFound()
    {
        var result = await CreateHandler().Handle(
            new ChangeUserRoleCommand(Guid.NewGuid(), nameof(UserRole.Admin)), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal("User.NotFound", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
