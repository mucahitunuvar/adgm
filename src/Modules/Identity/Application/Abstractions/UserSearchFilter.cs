using GenclikMerkezi.Modules.Identity.Domain;

namespace GenclikMerkezi.Modules.Identity.Application.Abstractions;

// Email is matched as a case-insensitive substring. Name-based search is not offered here: the
// User aggregate owns no name data (Identity is auth-only, per AGENTS.md §9's database-per-module
// rule Identity cannot query the Candidate/Employer databases where a profile name would live).
public sealed record UserSearchFilter(
    string? Email,
    UserRole? Role,
    UserStatus? Status,
    bool? IsLockedOut,
    bool? EmailConfirmed,
    int Page,
    int PageSize);
