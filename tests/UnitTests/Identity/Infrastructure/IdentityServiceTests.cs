using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;
using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Infrastructure;

public class IdentityServiceTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeSender _sender = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private IdentityService CreateService() => new(_userRepository, _sender, _unitOfWork);

    [Fact]
    public async Task GetUserProfileAsync_WithExistingUser_ReturnsProfile()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue("hash"),
            "Ahmet",
            "Yılmaz",
            "05551234567",
            UserRole.Candidate);
        _userRepository.Add(user);

        var profile = await CreateService().GetUserProfileAsync(user.Id, CancellationToken.None);

        Assert.NotNull(profile);
        Assert.Equal(user.Id, profile.UserId);
        Assert.Equal("aday@example.com", profile.Email);
        Assert.Equal("Ahmet", profile.FirstName);
        Assert.Equal("Yılmaz", profile.LastName);
        Assert.Equal("05551234567", profile.PhoneNumber);
    }

    [Fact]
    public async Task GetUserProfileAsync_WithUnknownUser_ReturnsNull()
    {
        var profile = await CreateService().GetUserProfileAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(profile);
    }

    [Fact]
    public async Task CreateUserAsync_OnSuccess_SendsRegisterUserCommandAndReturnsUserId()
    {
        var userId = Guid.NewGuid();
        _sender.SetResponse<RegisterUserCommand>(Result.Success(new RegisterUserResponse(userId, "aday@example.com", "Candidate")));

        var result = await CreateService().CreateUserAsync(
            "aday@example.com", "Sifre123", "Ahmet", "Yılmaz", "05551234567", "Candidate", CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value);
        var sentCommand = Assert.IsType<RegisterUserCommand>(_sender.LastRequest);
        Assert.Equal("aday@example.com", sentCommand.Email);
        Assert.Equal("Ahmet", sentCommand.FirstName);
        Assert.Equal("Candidate", sentCommand.Role);
    }

    [Fact]
    public async Task CreateUserAsync_OnFailure_PropagatesError()
    {
        var error = Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists.");
        _sender.SetResponse<RegisterUserCommand>(Result.Failure<RegisterUserResponse>(error));

        var result = await CreateService().CreateUserAsync(
            "aday@example.com", "Sifre123", "Ahmet", "Yılmaz", null, "Candidate", CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithExistingUser_DeactivatesAndSaves()
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value, PasswordHash.FromHashedValue("hash"), "Ahmet", "Yılmaz", null, UserRole.Candidate);
        _userRepository.Add(user);

        await CreateService().DeactivateUserAsync(user.Id, CancellationToken.None);

        Assert.Equal(UserStatus.Disabled, user.Status);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task DeactivateUserAsync_WithUnknownUser_DoesNotThrowOrSave()
    {
        var exception = await Record.ExceptionAsync(() => CreateService().DeactivateUserAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Null(exception);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
