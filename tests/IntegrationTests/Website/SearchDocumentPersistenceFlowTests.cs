using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Website.Application.Abstractions;
using GenclikMerkezi.Modules.Website.Domain;
using GenclikMerkezi.Modules.Website.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Website;

// ADR-024 §10 (Faz 5 Görev 1): SearchDocument/ISearchDocumentRepository have no feature/HTTP endpoint
// of their own yet (Görev 2 starts writing website content into them, Görev 3 starts reading from
// them, Görev 4 adds the external-source full sync) - this exercises the repository and the unique
// index directly, the same no-HTTP-endpoint pattern BotProtectionVerifierResolutionTests uses for a
// port nothing calls yet.
public class SearchDocumentPersistenceFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public SearchDocumentPersistenceFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static LanguageCode Tr() => LanguageCode.Create("tr").Value;

    private static SearchDocument NewDocument(string sourceKey, string sourceId, string title, DateTime now) =>
        SearchDocument.Create(
            sourceKey, sourceId, Tr(), "news", title, "Summary text", $"/haberler/{sourceId}",
            "NORMALIZED TEXT", now, now, true).Value;

    [Fact]
    public async Task UpsertAsync_WhenNoExistingDocument_Inserts()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var sourceId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        await repository.UpsertAsync(NewDocument("website", sourceId, "İlk Başlık", now));
        await dbContext.SaveChangesAsync();

        var stored = await repository.GetAsync("website", sourceId, Tr());
        Assert.NotNull(stored);
        Assert.Equal("İlk Başlık", stored!.Title);
    }

    [Fact]
    public async Task UpsertAsync_WhenDocumentAlreadyExists_RefreshesInPlace_WithoutCreatingASecondRow()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var sourceId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        await repository.UpsertAsync(NewDocument("website", sourceId, "İlk Başlık", now));
        await dbContext.SaveChangesAsync();

        await repository.UpsertAsync(NewDocument("website", sourceId, "Güncellenmiş Başlık", now.AddMinutes(5)));
        await dbContext.SaveChangesAsync();

        var stored = await repository.GetAsync("website", sourceId, Tr());
        Assert.NotNull(stored);
        Assert.Equal("Güncellenmiş Başlık", stored!.Title);
        Assert.Equal(1, await repository.CountBySourceAsync("website"));
    }

    [Fact]
    public async Task SourceKeySourceIdLanguageCode_IsUniqueAtTheDatabaseLevel()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var sourceId = Guid.NewGuid().ToString("N");
        var now = DateTime.UtcNow;
        dbContext.SearchDocuments.Add(NewDocument("website", sourceId, "Birinci", now));
        await dbContext.SaveChangesAsync();

        dbContext.SearchDocuments.Add(NewDocument("website", sourceId, "İkinci", now));
        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task DeleteUnseenBySourceAsync_RemovesOnlyDocumentsMissingFromTheSeenSet()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var sourceKey = $"employer.job-{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;
        var keptId = Guid.NewGuid().ToString("N");
        var droppedId = Guid.NewGuid().ToString("N");
        await repository.UpsertAsync(NewDocument(sourceKey, keptId, "Kalan", now));
        await repository.UpsertAsync(NewDocument(sourceKey, droppedId, "Düşen", now));
        await dbContext.SaveChangesAsync();

        var deleted = await repository.DeleteUnseenBySourceAsync(sourceKey, [keptId]);

        Assert.Equal(1, deleted);
        Assert.Equal(1, await repository.CountBySourceAsync(sourceKey));
        Assert.NotNull(await repository.GetAsync(sourceKey, keptId, Tr()));
        Assert.Null(await repository.GetAsync(sourceKey, droppedId, Tr()));
    }

    // PublicJobListingsEnabled = false (ADR-023 §6): the external source then returns zero documents
    // for that sync pass, and the full-sync job (Görev 4) must still clear everything it had indexed.
    [Fact]
    public async Task DeleteUnseenBySourceAsync_WhenSeenSetIsEmpty_RemovesTheWholeSource()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var sourceKey = $"employer.job-{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;
        await repository.UpsertAsync(NewDocument(sourceKey, Guid.NewGuid().ToString("N"), "Kapalı Bayrak", now));
        await dbContext.SaveChangesAsync();

        var deleted = await repository.DeleteUnseenBySourceAsync(sourceKey, []);

        Assert.Equal(1, deleted);
        Assert.Equal(0, await repository.CountBySourceAsync(sourceKey));
    }

    [Fact]
    public async Task DeleteBySourceAndIdsAsync_RemovesOnlyTheGivenIds()
    {
        using var scope = _factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchDocumentRepository>();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebsiteDbContext>();

        var sourceKey = $"website-{Guid.NewGuid():N}";
        var now = DateTime.UtcNow;
        var removedId = Guid.NewGuid().ToString("N");
        var remainingId = Guid.NewGuid().ToString("N");
        await repository.UpsertAsync(NewDocument(sourceKey, removedId, "Silinecek", now));
        await repository.UpsertAsync(NewDocument(sourceKey, remainingId, "Kalacak", now));
        await dbContext.SaveChangesAsync();

        var deleted = await repository.DeleteBySourceAndIdsAsync(sourceKey, [removedId]);

        Assert.Equal(1, deleted);
        Assert.Null(await repository.GetAsync(sourceKey, removedId, Tr()));
        Assert.NotNull(await repository.GetAsync(sourceKey, remainingId, Tr()));
    }

    // ADR-024 §10: "Başka projede kaynak kaydedilmezse job boş çalışır" - the port itself must resolve
    // cleanly, not throw, regardless of how many implementations the Host registers. Written when this
    // project had none yet (Görev 1); Görev 4 is where the Host registers its first one
    // (EmployerJobSearchSource), so this now proves the collection resolves to exactly that one adapter
    // rather than an empty one - the "no source registered" case itself has no project left to prove it
    // with, since Host always wires EmployerJobSearchSource in (PublicJobListingsEnabled gates its data,
    // not its registration).
    [Fact]
    public void IExternalSearchSource_ResolvesTheHostRegisteredEmployerAdapter()
    {
        using var scope = _factory.Services.CreateScope();
        var sources = scope.ServiceProvider.GetServices<IExternalSearchSource>();

        Assert.Equal(["employer.job"], sources.Select(s => s.SourceKey));
    }
}
