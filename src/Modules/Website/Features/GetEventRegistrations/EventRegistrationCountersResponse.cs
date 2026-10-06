namespace GenclikMerkezi.Modules.Website.Features.GetEventRegistrations;

// §1 "Üstte sayaç özeti (Confirmed/Applied/Waitlisted/kalan)" - Confirmed/Applied/Waitlisted are live
// counts of EventRegistration rows in that exact status; RemainingSpots is EventSchedule.Capacity minus
// ConfirmedCount (null when the event has no capacity ceiling), the same formula the public
// remainingSpots field (Görev 2) already uses.
public sealed record EventRegistrationCountersResponse(int Confirmed, int Applied, int Waitlisted, int? RemainingSpots);
