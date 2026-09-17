namespace GenclikMerkezi.Modules.Candidate;

public sealed class CandidateModuleMarker
{
    // Same reasoning as IdentityModuleMarker/ReferenceDataModuleMarker: each module resolves its own
    // IUnitOfWork by this key instead of a plain (unkeyed) registration, which would collide across
    // modules sharing one composition root.
    public const string UnitOfWorkKey = "Candidate";
}
