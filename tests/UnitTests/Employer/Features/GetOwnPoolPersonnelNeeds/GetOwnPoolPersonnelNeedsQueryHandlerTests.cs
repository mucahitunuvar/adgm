using GenclikMerkezi.Modules.Employer.Domain;
using GenclikMerkezi.Modules.Employer.Features.GetOwnPoolPersonnelNeeds;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.BuildingBlocks.TestDoubles;
using GenclikMerkezi.UnitTests.Candidate.TestDoubles;
using GenclikMerkezi.UnitTests.Employer.TestDoubles;

namespace GenclikMerkezi.UnitTests.Employer.Features.GetOwnPoolPersonnelNeeds;

public class GetOwnPoolPersonnelNeedsQueryHandlerTests
{
    private readonly FakePersonnelNeedRepository _personnelNeedRepository = new();
    private readonly FakeCareerAdvisorModuleContract _careerAdvisorModuleContract = new();
    private readonly Guid _advisorUserId = Guid.NewGuid();

    private GetOwnPoolPersonnelNeedsQueryHandler CreateHandler() =>
        new(_personnelNeedRepository, _careerAdvisorModuleContract, new FakeCurrentUserContext(_advisorUserId));

    private static PersonnelNeed CreateKendiHavuzundaPersonnelNeed(Guid companyId)
    {
        var personnelNeed = PersonnelNeed.Create(
            companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
        personnelNeed.Submit();
        return personnelNeed;
    }

    [Fact]
    public async Task Handle_ReturnsOnlyKendiHavuzundaPersonnelNeedsAssignedToCaller()
    {
        var careerAdvisorId = Guid.NewGuid();
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = careerAdvisorId;

        var myCompanyId = Guid.NewGuid();
        _personnelNeedRepository.RegisterCompanyCareerAdvisor(myCompanyId, careerAdvisorId);
        var myPersonnelNeed = CreateKendiHavuzundaPersonnelNeed(myCompanyId);
        _personnelNeedRepository.Add(myPersonnelNeed);

        var otherCompanyId = Guid.NewGuid();
        _personnelNeedRepository.RegisterCompanyCareerAdvisor(otherCompanyId, Guid.NewGuid());
        _personnelNeedRepository.Add(CreateKendiHavuzundaPersonnelNeed(otherCompanyId));

        var draftPersonnelNeed = PersonnelNeed.Create(
            myCompanyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], [], DateTime.UtcNow);
        _personnelNeedRepository.Add(draftPersonnelNeed);

        var result = await CreateHandler().Handle(new GetOwnPoolPersonnelNeedsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var returned = Assert.Single(result.Value);
        Assert.Equal(myPersonnelNeed.Id, returned.Id);
    }

    [Fact]
    public async Task Handle_WhenCallerIsNotACareerAdvisor_ReturnsForbidden()
    {
        _careerAdvisorModuleContract.AdvisorIdForCurrentUser = null;

        var result = await CreateHandler().Handle(new GetOwnPoolPersonnelNeedsQuery(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Forbidden, result.Error.Type);
    }
}
