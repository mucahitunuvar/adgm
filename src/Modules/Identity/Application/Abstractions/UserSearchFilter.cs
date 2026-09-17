using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

// Email is matched as a case-insensitive substring. Name-based search is not offered here even
// though User now carries FirstName/LastName (added for Candidate registration seeding) - this
// filter backs the admin user-management screen, which is Identity's own auth-focused concern;
// name search belongs on the consuming module's own profile screen (e.g. Candidate's CV search).
public sealed record UserSearchFilter(
    string? Email,
    UserRole? Role,
    UserStatus? Status,
    bool? IsLockedOut,
    bool? EmailConfirmed,
    int Page,
    int PageSize);
