using GenclikMerkezi.Modules.Employer.Domain;

namespace GenclikMerkezi.UnitTests.Employer.Domain;

public class PersonnelNeedTests
{
    private static PersonnelNeed CreateDraftPersonnelNeed(IReadOnlyCollection<Guid>? genderPreferenceIds = null) =>
        PersonnelNeed.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, Guid.NewGuid(),
            Guid.NewGuid(), "Acil ihtiyaç", genderPreferenceIds ?? [], [], [], [], DateTime.UtcNow);

    private static PersonnelNeed TransitionTo(PersonnelNeedStatus status)
    {
        var personnelNeed = CreateDraftPersonnelNeed();

        if (status == PersonnelNeedStatus.Taslak)
        {
            return personnelNeed;
        }

        personnelNeed.Submit();

        if (status == PersonnelNeedStatus.KendiHavuzunda)
        {
            return personnelNeed;
        }

        if (status == PersonnelNeedStatus.GenelHavuzda)
        {
            personnelNeed.PoolToGeneral(Guid.NewGuid(), DateTime.UtcNow);
            return personnelNeed;
        }

        personnelNeed.Close(Guid.NewGuid(), null, DateTime.UtcNow);
        return personnelNeed;
    }

    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToTaslak()
    {
        var genderId = Guid.NewGuid();

        var personnelNeed = CreateDraftPersonnelNeed([genderId]);

        Assert.Equal(PersonnelNeedStatus.Taslak, personnelNeed.Status);
        Assert.Equal(3, personnelNeed.Quantity);
        Assert.Single(personnelNeed.GenderPreferences);
        Assert.Equal(genderId, personnelNeed.GenderPreferences.Single().GenderId);
        Assert.Null(personnelNeed.PooledAtUtc);
        Assert.Null(personnelNeed.ClosedAtUtc);
    }

    [Fact]
    public void Submit_FromTaslak_Succeeds()
    {
        var personnelNeed = TransitionTo(PersonnelNeedStatus.Taslak);

        var result = personnelNeed.Submit();

        Assert.True(result.IsSuccess);
        Assert.Equal(PersonnelNeedStatus.KendiHavuzunda, personnelNeed.Status);
    }

    [Theory]
    [InlineData(PersonnelNeedStatus.KendiHavuzunda)]
    [InlineData(PersonnelNeedStatus.GenelHavuzda)]
    [InlineData(PersonnelNeedStatus.Karsilandi)]
    public void Submit_FromOtherStatuses_Fails(PersonnelNeedStatus initialStatus)
    {
        var personnelNeed = TransitionTo(initialStatus);

        var result = personnelNeed.Submit();

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, personnelNeed.Status);
    }

    [Fact]
    public void PoolToGeneral_FromKendiHavuzunda_Succeeds()
    {
        var personnelNeed = TransitionTo(PersonnelNeedStatus.KendiHavuzunda);
        var advisorId = Guid.NewGuid();
        var pooledAtUtc = DateTime.UtcNow;

        var result = personnelNeed.PoolToGeneral(advisorId, pooledAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(PersonnelNeedStatus.GenelHavuzda, personnelNeed.Status);
        Assert.Equal(advisorId, personnelNeed.PooledByAdvisorId);
        Assert.Equal(pooledAtUtc, personnelNeed.PooledAtUtc);
    }

    [Theory]
    [InlineData(PersonnelNeedStatus.Taslak)]
    [InlineData(PersonnelNeedStatus.GenelHavuzda)]
    [InlineData(PersonnelNeedStatus.Karsilandi)]
    public void PoolToGeneral_FromOtherStatuses_Fails(PersonnelNeedStatus initialStatus)
    {
        var personnelNeed = TransitionTo(initialStatus);

        var result = personnelNeed.PoolToGeneral(Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, personnelNeed.Status);
    }

    // Tasarım kararı (plan): Close hem KendiHavuzunda hem GenelHavuzda'dan çağrılabilir - kendi
    // danışmanı havuza atmadan da kendi adayıyla karşılayabilir.
    [Theory]
    [InlineData(PersonnelNeedStatus.KendiHavuzunda)]
    [InlineData(PersonnelNeedStatus.GenelHavuzda)]
    public void Close_FromKendiHavuzundaOrGenelHavuzda_Succeeds(PersonnelNeedStatus initialStatus)
    {
        var personnelNeed = TransitionTo(initialStatus);
        var closedByAdvisorId = Guid.NewGuid();
        var candidateCvId = Guid.NewGuid();
        var closedAtUtc = DateTime.UtcNow;

        var result = personnelNeed.Close(closedByAdvisorId, candidateCvId, closedAtUtc);

        Assert.True(result.IsSuccess);
        Assert.Equal(PersonnelNeedStatus.Karsilandi, personnelNeed.Status);
        Assert.Equal(closedByAdvisorId, personnelNeed.ClosedByAdvisorId);
        Assert.Equal(candidateCvId, personnelNeed.FulfilledByCandidateCvId);
        Assert.Equal(closedAtUtc, personnelNeed.ClosedAtUtc);
    }

    [Theory]
    [InlineData(PersonnelNeedStatus.Taslak)]
    [InlineData(PersonnelNeedStatus.Karsilandi)]
    public void Close_FromOtherStatuses_Fails(PersonnelNeedStatus initialStatus)
    {
        var personnelNeed = TransitionTo(initialStatus);

        var result = personnelNeed.Close(Guid.NewGuid(), null, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, personnelNeed.Status);
    }

    [Theory]
    [InlineData(PersonnelNeedStatus.Taslak)]
    public void Update_FromTaslak_Succeeds(PersonnelNeedStatus initialStatus)
    {
        var personnelNeed = TransitionTo(initialStatus);
        var newGenderId = Guid.NewGuid();

        var result = personnelNeed.Update(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, Guid.NewGuid(),
            Guid.NewGuid(), "Güncellenmiş açıklama", [newGenderId], [], [], []);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, personnelNeed.Quantity);
        Assert.Equal("Güncellenmiş açıklama", personnelNeed.DetailsText);
        Assert.Single(personnelNeed.GenderPreferences);
        Assert.Equal(newGenderId, personnelNeed.GenderPreferences.Single().GenderId);
    }

    [Theory]
    [InlineData(PersonnelNeedStatus.KendiHavuzunda)]
    [InlineData(PersonnelNeedStatus.GenelHavuzda)]
    [InlineData(PersonnelNeedStatus.Karsilandi)]
    public void Update_FromOtherStatuses_Fails(PersonnelNeedStatus initialStatus)
    {
        var personnelNeed = TransitionTo(initialStatus);

        var result = personnelNeed.Update(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5, Guid.NewGuid(),
            Guid.NewGuid(), null, [], [], [], []);

        Assert.True(result.IsFailure);
        Assert.Equal(initialStatus, personnelNeed.Status);
    }

    [Fact]
    public void SetGenderPreferences_ReplacesExistingPreferences()
    {
        var firstGenderId = Guid.NewGuid();
        var secondGenderId = Guid.NewGuid();
        var personnelNeed = CreateDraftPersonnelNeed([firstGenderId]);

        personnelNeed.SetGenderPreferences([secondGenderId]);

        Assert.Single(personnelNeed.GenderPreferences);
        Assert.Equal(secondGenderId, personnelNeed.GenderPreferences.Single().GenderId);
    }
}
