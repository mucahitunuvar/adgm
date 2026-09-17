using System.Security.Claims;
using GenclikMerkezi.BuildingBlocks.Infrastructure.Security;
using Microsoft.AspNetCore.Http;

namespace GenclikMerkezi.UnitTests.BuildingBlocks.Security;

public class HttpContextCurrentUserContextTests
{
    [Fact]
    public void UserId_WithNameIdentifierClaim_ReturnsParsedGuid()
    {
        var userId = Guid.NewGuid();
        var accessor = CreateAccessor(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));

        var context = new HttpContextCurrentUserContext(accessor);

        Assert.Equal(userId, context.UserId);
    }

    [Fact]
    public void UserId_WithoutNameIdentifierClaim_ReturnsNull()
    {
        var accessor = CreateAccessor();

        var context = new HttpContextCurrentUserContext(accessor);

        Assert.Null(context.UserId);
    }

    [Fact]
    public void UserId_WithNoHttpContext_ReturnsNull()
    {
        var accessor = new HttpContextAccessor { HttpContext = null };

        var context = new HttpContextCurrentUserContext(accessor);

        Assert.Null(context.UserId);
    }

    private static IHttpContextAccessor CreateAccessor(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        return new HttpContextAccessor { HttpContext = httpContext };
    }
}
