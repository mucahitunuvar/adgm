namespace GenclikMerkezi.UnitTests.CareerAdvisor.Domain;

public class CareerAdvisorTests
{
    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToActive()
    {
        var userId = Guid.NewGuid();
        var createdAtUtc = DateTime.UtcNow;

        var careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            userId, "Ayşe", "Kaya", "danisman@example.com", "05551234567", createdAtUtc);

        Assert.Equal(userId, careerAdvisor.UserId);
        Assert.Equal("Ayşe", careerAdvisor.FirstName);
        Assert.Equal("Kaya", careerAdvisor.LastName);
        Assert.Equal("danisman@example.com", careerAdvisor.Email);
        Assert.Equal("05551234567", careerAdvisor.PhoneNumber);
        Assert.Equal(createdAtUtc, careerAdvisor.CreatedAtUtc);
        Assert.True(careerAdvisor.IsActive);
        Assert.Null(careerAdvisor.DeactivatedAtUtc);
    }

    [Fact]
    public void Deactivate_OnActiveAdvisor_SetsInactiveAndDeactivatedAtUtc()
    {
        var careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            Guid.NewGuid(), "Ayşe", "Kaya", "danisman@example.com", null, DateTime.UtcNow);
        var deactivatedAtUtc = DateTime.UtcNow;

        careerAdvisor.Deactivate(deactivatedAtUtc);

        Assert.False(careerAdvisor.IsActive);
        Assert.Equal(deactivatedAtUtc, careerAdvisor.DeactivatedAtUtc);
    }

    [Fact]
    public void Deactivate_OnAlreadyInactiveAdvisor_IsNoOp_AndPreservesOriginalDeactivatedAtUtc()
    {
        var careerAdvisor = GenclikMerkezi.Modules.CareerAdvisor.Domain.CareerAdvisor.Create(
            Guid.NewGuid(), "Ayşe", "Kaya", "danisman@example.com", null, DateTime.UtcNow);
        var firstDeactivation = DateTime.UtcNow;
        careerAdvisor.Deactivate(firstDeactivation);

        careerAdvisor.Deactivate(firstDeactivation.AddDays(1));

        Assert.False(careerAdvisor.IsActive);
        Assert.Equal(firstDeactivation, careerAdvisor.DeactivatedAtUtc);
    }
}
