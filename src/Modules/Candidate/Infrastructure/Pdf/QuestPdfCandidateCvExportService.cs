using GenclikMerkezi.Modules.Candidate.Application.Abstractions;
using QuestPDF.Fluent;

namespace GenclikMerkezi.Modules.Candidate.Infrastructure.Pdf;

public sealed class QuestPdfCandidateCvExportService : ICandidateCvPdfExportService
{
    public byte[] Generate(CandidateCvPdfModel model) => new CvPdfDocument(model).GeneratePdf();
}
