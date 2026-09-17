using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.ResetPassword;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakePasswordResetTokenGenerator _tokenGenerator = new();
    private readonly FakePasswordHasher _passwordHasher = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private ResetPasswordCommandHandler CreateHandler() =>
        new(_userRepository, _tokenGenerator, _passwordHasher, _unitOfWork);

    private (User User, string PlainToken) AddUserWithResetToken(DateTime expiresAtUtc)
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue(_passwordHasher.Hash("OldPass123")), "Test", "User", null,
            UserRole.Candidate);

        const string plainToken = "known-reset-token";
        user.IssuePasswordResetToken(_tokenGenerator.Hash(plainToken), expiresAtUtc);

        _userRepository.Add(user);
        return (user, plainToken);
    }

    [Fact]
    public async Task Handle_WithValidToken_UpdatesPasswordAndConsumesToken()
    {
        var (user, plainToken) = AddUserWithResetToken(DateTime.UtcNow.AddMinutes(30));

        var result = await CreateHandler().Handle(
            new ResetPasswordCommand(plainToken, "NewPass456"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(_passwordHasher.Verify("NewPass456", user.PasswordHash.Value));
        Assert.True(user.FindPasswordResetToken(_tokenGenerator.Hash(plainToken))!.IsUsed);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsInvalidResetToken()
    {
        var result = await CreateHandler().Handle(
            new ResetPasswordCommand("never-issued-token", "NewPass456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidResetToken", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsInvalidResetToken()
    {
        var (_, plainToken) = AddUserWithResetToken(DateTime.UtcNow.AddSeconds(-1));

        var result = await CreateHandler().Handle(
            new ResetPasswordCommand(plainToken, "NewPass456"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidResetToken", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithAlreadyUsedToken_ReturnsInvalidResetToken()
    {
        var (_, plainToken) = AddUserWithResetToken(DateTime.UtcNow.AddMinutes(30));
        await CreateHandler().Handle(new ResetPasswordCommand(plainToken, "FirstNewPass1"), CancellationToken.None);

        var result = await CreateHandler().Handle(
            new ResetPasswordCommand(plainToken, "SecondNewPass1"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidResetToken", result.Error.Code);
    }
}
