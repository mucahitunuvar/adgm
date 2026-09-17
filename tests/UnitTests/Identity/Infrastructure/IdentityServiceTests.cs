using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.UnitTests.Identity.TestDoubles;

namespace GenclikMerkezi.UnitTests.Identity.Infrastructure;

public class IdentityServiceTests
{
    private readonly FakeUserRepository _userRepository = new();

    private IdentityService CreateService() => new(_userRepository);

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
}
