using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Features.VerifyEmail;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Features.VerifyEmail;

public class VerifyEmailCommandHandlerTests
{
    private readonly FakeUserRepository _userRepository = new();
    private readonly FakeEmailVerificationTokenGenerator _tokenGenerator = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private VerifyEmailCommandHandler CreateHandler() =>
        new(_userRepository, _tokenGenerator, _unitOfWork);

    private (User User, string PlainToken) AddUserWithVerificationToken(DateTime expiresAtUtc)
    {
        var user = User.Register(
            Email.Create("aday@example.com").Value,
            PasswordHash.FromHashedValue("hash"), "Test", "User", null,
            UserRole.Candidate);

        const string plainToken = "known-verification-token";
        user.IssueEmailVerificationToken(_tokenGenerator.Hash(plainToken), expiresAtUtc);

        _userRepository.Add(user);
        return (user, plainToken);
    }

    [Fact]
    public async Task Handle_WithValidToken_ConfirmsEmailAndConsumesToken()
    {
        var (user, plainToken) = AddUserWithVerificationToken(DateTime.UtcNow.AddHours(48));

        var result = await CreateHandler().Handle(new VerifyEmailCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.EmailConfirmed);
        Assert.True(user.FindEmailVerificationToken(_tokenGenerator.Hash(plainToken))!.IsUsed);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownToken_ReturnsInvalidVerificationToken()
    {
        var result = await CreateHandler().Handle(
            new VerifyEmailCommand("never-issued-token"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
        Assert.Equal("Auth.InvalidVerificationToken", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithExpiredToken_ReturnsInvalidVerificationToken()
    {
        var (_, plainToken) = AddUserWithVerificationToken(DateTime.UtcNow.AddSeconds(-1));

        var result = await CreateHandler().Handle(new VerifyEmailCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Auth.InvalidVerificationToken", result.Error.Code);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WhenAlreadyConfirmed_IsIdempotentAndSucceeds()
    {
        var (_, plainToken) = AddUserWithVerificationToken(DateTime.UtcNow.AddHours(48));
        await CreateHandler().Handle(new VerifyEmailCommand(plainToken), CancellationToken.None);

        var result = await CreateHandler().Handle(new VerifyEmailCommand(plainToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }
}
