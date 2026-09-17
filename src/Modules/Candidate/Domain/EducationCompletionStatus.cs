namespace GenclikMerkezi.Modules.Candidate.Domain;

// Candidate.md's Eğitim Bilgileri section literally lists only two options ("Devam Ediyorum" /
// "Terk"), but that leaves no way to represent an education actually completed (graduated) without
// misrepresenting it as "Terk" (dropped out) - a materially different fact for an employer reading
// the CV. Graduated is added as the third, "neither still enrolled nor dropped out" state; this is
// a domain-model correction over the literal source list, not a new business feature. An Education
// entry's own lifecycle state, not a shared reference list, so it lives here as a plain domain enum
// rather than a ReferenceData lookup.
public enum EducationCompletionStatus
{
    Continuing,
    Graduated,
    Dropped,
}
