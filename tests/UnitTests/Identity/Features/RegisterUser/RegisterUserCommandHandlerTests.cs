using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.RegisterUser;

public class RegisterUserCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private RegisterUserCommandHandler CreateHandler() =>
        new(_userRepository, _passwordHasher, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidInput_CreatesUserAndReturnsSuccess()
    {
        var command = new RegisterUserCommand("aday@example.com", "Sifre123", "Candidate");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("aday@example.com", result.Value.Email);
        Assert.Equal("Candidate", result.Value.Role);
        Assert.Single(_userRepository.Users);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsValidationFailure_AndDoesNotPersist()
    {
        var command = new RegisterUserCommand("not-an-email", "Sifre123", "Candidate");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Empty(_userRepository.Users);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyRegisteredEmail_ReturnsConflict_AndDoesNotPersistAgain()
    {
        var existingUser = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue("hash"),
            UserRole.Candidate);
        _userRepository.Add(existingUser);

        var command = new RegisterUserCommand("aday@example.com", "Sifre123", "Candidate");

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Single(_userRepository.Users);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
