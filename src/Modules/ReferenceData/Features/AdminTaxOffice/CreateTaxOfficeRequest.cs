namespace GenclikMerkezi.Modules.ReferenceData.Features.AdminTaxOffice;

public sealed record CreateTaxOfficeRequest(string Code, string DisplayName, int SortOrder, Guid ProvinceId);
