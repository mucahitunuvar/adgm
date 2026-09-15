using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.UnitTests.Identity.TestDoubles;

public sealed class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public AccessToken GenerateAccessToken(User user)
    {
        return new AccessToken($"access-token-for-{user.Id}", DateTime.UtcNow.AddMinutes(15));
    }
}
