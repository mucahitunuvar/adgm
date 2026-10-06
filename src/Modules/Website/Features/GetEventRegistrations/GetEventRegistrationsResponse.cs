using GenclikMerkezi.SharedKernel.Results;

namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;

public sealed record GetEventRegistrationsResponse(
    EventRegistrationCountersResponse Counters, PagedResult<EventRegistrationSummaryResponse> Registrations);
