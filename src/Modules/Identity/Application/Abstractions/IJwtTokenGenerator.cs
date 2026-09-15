using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

public interface IJwtTokenGenerator
{
    AccessToken GenerateAccessToken(User user);
}
