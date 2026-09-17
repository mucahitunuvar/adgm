using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.UnitTests.ReferenceData.Domain;

// Exercised through Sector - a plain admin-managed lookup - since LookupItem itself is abstract and
// every concrete type shares the exact same base behavior.
public class LookupItemTests
{
    [Fact]
    public void Create_SetsAllFieldsAndDefaultsToActive()
    {
        var sector = Sector.Create("IT", "Bilişim", 1);

        Assert.NotEqual(Guid.Empty, sector.Id);
        Assert.Equal("IT", sector.Code);
        Assert.Equal("Bilişim", sector.DisplayName);
        Assert.Equal(1, sector.SortOrder);
        Assert.True(sector.IsActive);
    }

    [Fact]
    public void Update_ChangesDisplayNameAndSortOrder_ButNotCodeOrId()
    {
        var sector = Sector.Create("IT", "Bilişim", 1);
        var originalId = sector.Id;

        sector.Update("Bilişim Teknolojileri", 5);

        Assert.Equal(originalId, sector.Id);
        Assert.Equal("IT", sector.Code);
        Assert.Equal("Bilişim Teknolojileri", sector.DisplayName);
        Assert.Equal(5, sector.SortOrder);
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse_WithoutClearingOtherFields()
    {
        var sector = Sector.Create("IT", "Bilişim", 1);

        sector.Deactivate();

        Assert.False(sector.IsActive);
        Assert.Equal("IT", sector.Code);
        Assert.Equal("Bilişim", sector.DisplayName);
    }

    [Fact]
    public void Activate_AfterDeactivate_SetsIsActiveTrueAgain()
    {
        var sector = Sector.Create("IT", "Bilişim", 1);
        sector.Deactivate();

        sector.Activate();

        Assert.True(sector.IsActive);
    }

    [Fact]
    public void LookupType_MatchesTheConcreteType()
    {
        Assert.Equal(GenclikMerkezi.Contracts.ReferenceData.ReferenceDataLookupType.Sector, Sector.LookupType);
    }
}
