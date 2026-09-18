namespace GenclikMerkezi.Modules.Candidate.Application.Abstractions;

// Abstraction over the QuestPDF-based renderer (ADR-021) - the Feature handler depends on this, not
// on Candidate.Infrastructure's CvPdfDocument/QuestPDF directly (AGENTS.md §7: Application must not
// depend on concrete Infrastructure implementations).
public interface ICandidateCvPdfExportService
{
    byte[] Generate(CandidateCvPdfModel model);
}
