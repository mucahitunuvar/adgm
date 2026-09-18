using GenclikMerkezi.Modules.CareerAdvisor.Features.DeactivateCareerAdvisor;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.DeactivateCareerAdvisor;

public class DeactivateCareerAdvisorCommandHandlerTests
{
    private readonly FakeCareerAdvisorRepository _careerAdvisorRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private DeactivateCareerAdvisorCommandHandler CreateHandler() =>
        new(_careerAdvisorRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithExistingActiveAdvisor_DeactivatesAndSaves()
    {
        var careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            Guid.NewGuid(), "Ayşe", "Kaya", "danisman@example.com", null, DateTime.UtcNow);
        _careerAdvisorRepository.Add(careerAdvisor);

        var result = await CreateHandler().Handle(new DeactivateCareerAdvisorCommand(careerAdvisor.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(careerAdvisor.IsActive);
        Assert.NotNull(careerAdvisor.DeactivatedAtUtc);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task Handle_WithUnknownId_ReturnsNotFound_AndDoesNotSave()
    {
        var result = await CreateHandler().Handle(new DeactivateCareerAdvisorCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }
}
