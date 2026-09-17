using GenclikMerkezi.BuildingBlocks.Infrastructure.Persistence;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Identity.Domain;
using GenclikMerkezi.Modules.Identity.Infrastructure;
using GenclikMerkezi.SharedKernel.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.BuildingBlocks;

// Exercises QueryablePagingExtensions.ToPagedResultAsync directly against a real EF Core provider
// (IdentityDbContext/LocalDB, via the shared CustomWebApplicationFactory) - the two repositories
// that consume it (UserRepository, AdminAuditLogRepository) are already covered end-to-end by
// AdminGetUsersFlowTests/AdminGetAuditLogFlowTests, this covers the extension's own edge cases
// (page boundaries, empty result set) directly.
public class QueryablePagingExtensionsTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<List<Guid>> SeedUsersAsync(IdentityDbContext dbContext, int count)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var ids = new List<Guid>();

        for (var i = 0; i < count; i++)
        {
            var user = User.Register(
                Email.Create($"paging-{suffix}-{i}@example.com").Value,
                PasswordHash.FromHashedValue("hash"), "Test", "User", null,
                UserRole.Candidate);
            dbContext.Users.Add(user);
            ids.Add(user.Id);
        }

        await dbContext.SaveChangesAsync();
        return ids;
    }

    [Fact]
    public async Task ToPagedResultAsync_ReturnsCorrectSliceAndTotalCount()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var ids = await SeedUsersAsync(dbContext, count: 5);

        var page1 = await dbContext.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .OrderBy(u => u.CreatedAtUtc)
            .ToPagedResultAsync(new PagedRequest { Page = 1, PageSize = 2 }, CancellationToken.None);

        Assert.Equal(5, page1.TotalCount);
        Assert.Equal(2, page1.Items.Count);
        Assert.Equal(3, page1.TotalPages);
        Assert.True(page1.HasNextPage);
        Assert.False(page1.HasPreviousPage);
    }

    [Fact]
    public async Task ToPagedResultAsync_OnLastPartialPage_ReturnsRemainingItemsOnly()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        var ids = await SeedUsersAsync(dbContext, count: 5);

        var lastPage = await dbContext.Users.AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .OrderBy(u => u.CreatedAtUtc)
            .ToPagedResultAsync(new PagedRequest { Page = 3, PageSize = 2 }, CancellationToken.None);

        Assert.Single(lastPage.Items);
        Assert.False(lastPage.HasNextPage);
        Assert.True(lastPage.HasPreviousPage);
    }

    [Fact]
    public async Task ToPagedResultAsync_WithNoMatchingRows_ReturnsEmptyResultWithoutQueryingRows()
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var result = await dbContext.Users.AsNoTracking()
            .Where(u => u.Id == Guid.NewGuid())
            .ToPagedResultAsync(new PagedRequest(), CancellationToken.None);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalPages);
    }
}
