using System.Security.Claims;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.AspNetCore.Http;

namespace GenclikMerkezi.BuildingBlocks.Infrastructure.Security;

// Reads the same ClaimTypes.NameIdentifier claim Identity's own CurrentUserService reads for its
// UserId property - deliberately not a shared implementation with it (ADR-017 Decision 2): this
// type must not depend on the Identity module, so it duplicates that one claim read instead.
public sealed class HttpContextCurrentUserContext(IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }
}
