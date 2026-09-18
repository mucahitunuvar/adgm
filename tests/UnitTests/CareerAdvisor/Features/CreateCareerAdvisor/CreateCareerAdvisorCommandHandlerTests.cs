using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.SharedKernel.Results;
using GenclikMerkezi.UnitTests.CareerAdvisor.TestDoubles;

namespace GenclikMerkezi.UnitTests.CareerAdvisor.Features.CreateCareerAdvisor;

public class CreateCareerAdvisorCommandHandlerTests
{
    private readonly FakeIdentityService _identityService = new();
    private readonly FakeCareerAdvisorRepository _careerAdvisorRepository = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private CreateCareerAdvisorCommandHandler CreateHandler() =>
        new(_identityService, _careerAdvisorRepository, _unitOfWork);

    private static CreateCareerAdvisorCommand ValidCommand() =>
        new("danisman@example.com", "Sifre123", "Ayşe", "Kaya", "05551234567");

    [Fact]
    public async Task Handle_WithValidInput_CreatesUserThenCareerAdvisor()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateStaffUserResult = Result.Success(userId);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);

        var careerAdvisor = Assert.Single(_careerAdvisorRepository.CareerAdvisors);
        Assert.Equal(userId, careerAdvisor.UserId);
        Assert.Equal("Ayşe", careerAdvisor.FirstName);
        Assert.Equal("Kaya", careerAdvisor.LastName);
        Assert.Equal("danisman@example.com", careerAdvisor.Email);
        Assert.Equal(careerAdvisor.Id, result.Value.CareerAdvisorId);

        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.False(_identityService.DeactivateUserAsyncCalled);
    }

    [Fact]
    public async Task Handle_WhenIdentityServiceReportsEmailAlreadyExists_ReturnsFailure_AndDoesNotCreateCareerAdvisor()
    {
        var error = Error.Conflict("User.EmailAlreadyExists", "A user with this email already exists.");
        _identityService.CreateStaffUserResult = Result.Failure<Guid>(error);

        var result = await CreateHandler().Handle(ValidCommand(), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
        Assert.Empty(_careerAdvisorRepository.CareerAdvisors);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
        Assert.False(_identityService.DeactivateUserAsyncCalled);
    }

    [Fact]
    public async Task Handle_WhenCareerAdvisorPersistenceFails_DeactivatesUser_AndRethrows()
    {
        var userId = Guid.NewGuid();
        _identityService.CreateStaffUserResult = Result.Success(userId);
        _unitOfWork.ThrowOnSave = new InvalidOperationException("Simulated CareerAdvisor persistence failure.");

        await Assert.ThrowsAsync<InvalidOperationException>(() => CreateHandler().Handle(ValidCommand(), CancellationToken.None));

        Assert.True(_identityService.DeactivateUserAsyncCalled);
        Assert.Equal(userId, _identityService.DeactivatedUserId);
    }
}
