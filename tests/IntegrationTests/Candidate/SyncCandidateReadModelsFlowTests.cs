using System.Net.Http.Json;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.Modules.Candidate;
using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.SharedKernel.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.Candidate;

// No update endpoint exists yet for most of these fields (that lands in a later task) - these tests
// instead resolve Candidate's real repositories/DbContext from the running app's DI container and
// call the aggregates' own mutation methods directly, then assert against the same real database.
// This still exercises the real end-to-end pipeline this task adds (domain event -> MediatR dispatch
// -> CandidateDbContext.SaveChangesAsync -> CandidateCv[Content]UpdatedSyncsReadModelsHandler ->
// second SaveChangesAsync), not a mock of any part of it.
public class SyncCandidateReadModelsFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SyncCandidateReadModelsFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<Guid> RegisterCandidateAsync(string firstName = "Ahmet", string lastName = "Yılmaz")
    {
        var email = $"aday-{Guid.NewGuid():N}@example.com";
        var response = await _client.PostAsJsonAsync(
            "/api/v1/candidates/register",
            new { email, password = "Sifre123", firstName, lastName, phoneNumber = (string?)null });
        var body = await response.Content.ReadFromJsonAsync<RegisterCandidateResponse>();
        return body!.CandidateCvId;
    }

    [Fact]
    public async Task UpdatingCandidateCv_TriggersRecalculation_AndPersistsNewPercentage()
    {
        var candidateCvId = await RegisterCandidateAsync();

        using var scope = _factory.Services.CreateScope();
        var candidateCvRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var candidateCv = await candidateCvRepository.GetByIdAsync(candidateCvId);
        Assert.NotNull(candidateCv);
        Assert.Equal(0, candidateCv!.CompletionPercentage);

        candidateCv.AddSocialMediaLink("GitHub", "https://github.com/ahmet");
        await unitOfWork.SaveChangesAsync();

        using var verifyScope = _factory.Services.CreateScope();
        var verifyRepository = verifyScope.ServiceProvider.GetRequiredService<ICandidateCvRepository>();
        var reloaded = await verifyRepository.GetByIdAsync(candidateCvId);

        Assert.NotNull(reloaded);
        Assert.True(reloaded!.CompletionPercentage > 0);
    }

    [Fact]
    public async Task UpdatingCandidateCvContent_TriggersRecalculation_AndPersistsNewPercentageOnCandidateCv()
    {
        var candidateCvId = await RegisterCandidateAsync();

        using var scope = _factory.Services.CreateScope();
        var candidateCvContentRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvContentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCvId);
        Assert.NotNull(content);

        content!.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));
        await unitOfWork.SaveChangesAsync();

        using var verifyScope = _factory.Services.CreateScope();
        var candidateCvRepository = verifyScope.ServiceProvider.GetRequiredService<ICandidateCvRepository>();
        var reloadedCv = await candidateCvRepository.GetByIdAsync(candidateCvId);

        Assert.NotNull(reloadedCv);
        Assert.True(reloadedCv!.CompletionPercentage > 0);
    }

    [Fact]
    public async Task RegisteringCandidate_CreatesCandidateSearchIndexRow_Immediately()
    {
        var candidateCvId = await RegisterCandidateAsync("İrem", "Şahin");

        using var scope = _factory.Services.CreateScope();
        var searchIndexRepository = scope.ServiceProvider.GetRequiredService<ICandidateSearchIndexRepository>();

        var index = await searchIndexRepository.GetByCandidateCvIdAsync(candidateCvId);

        Assert.NotNull(index);
        Assert.Equal("IREM SAHIN", index!.FullNameNormalized);
        Assert.Equal(0, index.CompletionPercentage);
        Assert.Empty(index.EducationLevelIds);
        Assert.Empty(index.SectorIds);
    }

    [Fact]
    public async Task UpdatingContactInfo_UpdatesSearchIndexFullNameNormalizedEmailAndCompletionPercentage()
    {
        var candidateCvId = await RegisterCandidateAsync();

        using var scope = _factory.Services.CreateScope();
        var candidateCvRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var candidateCv = await candidateCvRepository.GetByIdAsync(candidateCvId);
        candidateCv!.UpdateContactInfo(
            "Ayşe", "Öztürk", "ayse.ozturk@example.com", null, null, null, null, "Kadıköy, İstanbul");
        await unitOfWork.SaveChangesAsync();

        using var verifyScope = _factory.Services.CreateScope();
        var searchIndexRepository = verifyScope.ServiceProvider.GetRequiredService<ICandidateSearchIndexRepository>();
        var index = await searchIndexRepository.GetByCandidateCvIdAsync(candidateCvId);

        Assert.NotNull(index);
        Assert.Equal("AYSE OZTURK", index!.FullNameNormalized);
        Assert.Equal("ayse.ozturk@example.com", index.Email);
        Assert.True(index.CompletionPercentage > 0);
    }

    [Fact]
    public async Task AddingExperienceWithSector_UpdatesSearchIndexSectorIds()
    {
        var candidateCvId = await RegisterCandidateAsync();
        var sectorId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var candidateCvContentRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvContentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCvId);
        content!.AddExperience("Acme A.Ş.", new DateOnly(2020, 1, 1));
        var experience = content.Experiences.First();
        experience.Update(
            "Acme A.Ş.", null, new DateOnly(2020, 1, 1), null, true, sectorId, null, null, null, null, null);
        await unitOfWork.SaveChangesAsync();

        using var verifyScope = _factory.Services.CreateScope();
        var searchIndexRepository = verifyScope.ServiceProvider.GetRequiredService<ICandidateSearchIndexRepository>();
        var index = await searchIndexRepository.GetByCandidateCvIdAsync(candidateCvId);

        Assert.NotNull(index);
        Assert.Equal([sectorId], index!.SectorIds);
    }

    [Fact]
    public async Task AddingEducation_UpdatesSearchIndexEducationLevelIds()
    {
        var candidateCvId = await RegisterCandidateAsync();
        var educationLevelId = Guid.NewGuid();

        using var scope = _factory.Services.CreateScope();
        var candidateCvContentRepository = scope.ServiceProvider.GetRequiredService<ICandidateCvContentRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(CandidateModuleMarker.UnitOfWorkKey);

        var content = await candidateCvContentRepository.GetByCandidateCvIdAsync(candidateCvId);
        content!.AddEducation(educationLevelId, new DateOnly(2016, 9, 1));
        await unitOfWork.SaveChangesAsync();

        using var verifyScope = _factory.Services.CreateScope();
        var searchIndexRepository = verifyScope.ServiceProvider.GetRequiredService<ICandidateSearchIndexRepository>();
        var index = await searchIndexRepository.GetByCandidateCvIdAsync(candidateCvId);

        Assert.NotNull(index);
        Assert.Equal([educationLevelId], index!.EducationLevelIds);
    }
}
