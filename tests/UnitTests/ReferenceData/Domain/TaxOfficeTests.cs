using GenclikMerkezi.Modules.ReferenceData.Domain;

namespace GenclikMerkezi.UnitTests.ReferenceData.Domain;

public class TaxOfficeTests
{
    [Fact]
    public void Create_SetsProvinceId()
    {
        var provinceId = Guid.NewGuid();

        var taxOffice = TaxOffice.Create("1250", "Adana İhtisas Vergi Dairesi", 0, provinceId);

        Assert.Equal(provinceId, taxOffice.ProvinceId);
    }

    [Fact]
    public void Deactivate_DoesNotClearProvinceId()
    {
        var provinceId = Guid.NewGuid();
        var taxOffice = TaxOffice.Create("1250", "Adana İhtisas Vergi Dairesi", 0, provinceId);

        taxOffice.Deactivate();

        Assert.False(taxOffice.IsActive);
        Assert.Equal(provinceId, taxOffice.ProvinceId);
    }

    [Fact]
    public void Update_DoesNotChangeProvinceId()
    {
        var provinceId = Guid.NewGuid();
        var taxOffice = TaxOffice.Create("1250", "Adana İhtisas Vergi Dairesi", 0, provinceId);

        taxOffice.Update("Yeni İsim", 3);

        Assert.Equal(provinceId, taxOffice.ProvinceId);
        Assert.Equal("Yeni İsim", taxOffice.DisplayName);
    }
}
