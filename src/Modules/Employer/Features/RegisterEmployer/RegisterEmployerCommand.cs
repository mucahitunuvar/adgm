using GenclikMerkezi.SharedKernel.Results;
using MediatR;

namespace GenclikMerkezi.Modules.Employer.Features.RegisterEmployer;

public sealed record RegisterEmployerCommand(
    string Email,
    string Password,
    string Name,
    Guid SectorId,
    int? FoundedYear,
    int? EmployeeCount,
    string? WebsiteUrl,
    Guid CountryId,
    Guid ProvinceId,
    Guid DistrictId,
    string Address,
    string? AboutHtml,
    string ContactFirstName,
    string ContactLastName,
    string ContactPhone,
    Guid TaxOfficeId,
    string TaxNumber,
    bool MarketingConsent) : IRequest<Result<RegisterEmployerResponse>>;
