using GenclikMerkezi.Modules.Candidate.Features.AddCandidateLanguage;
using GenclikMerkezi.Modules.Candidate.Features.AdminReassignCandidate;
using GenclikMerkezi.Modules.Candidate.Features.AddCandidateReference;
using GenclikMerkezi.Modules.Candidate.Features.AddCertificate;
using GenclikMerkezi.Modules.Candidate.Features.AddEducation;
using GenclikMerkezi.Modules.Candidate.Features.AddExperience;
using GenclikMerkezi.Modules.Candidate.Features.ConfirmMeeting;
using GenclikMerkezi.Modules.Candidate.Features.ExportCandidateCvPdf;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCv;
using GenclikMerkezi.Modules.Candidate.Features.GetCandidateCvContent;
using GenclikMerkezi.Modules.Candidate.Features.RegisterCandidate;
using GenclikMerkezi.Modules.Candidate.Features.RemoveCandidateLanguage;
using GenclikMerkezi.Modules.Candidate.Features.RemoveCandidateReference;
using GenclikMerkezi.Modules.Candidate.Features.RemoveCertificate;
using GenclikMerkezi.Modules.Candidate.Features.RemoveEducation;
using GenclikMerkezi.Modules.Candidate.Features.RemoveExperience;
using GenclikMerkezi.Modules.Candidate.Features.RequestMeeting;
using GenclikMerkezi.Modules.Candidate.Features.SearchCandidates;
using GenclikMerkezi.Modules.Candidate.Features.SendBulkCandidateNotification;
using GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContactInfo;
using GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvContentSummary;
using GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateCvPersonalInfo;
using GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateLanguage;
using GenclikMerkezi.Modules.Candidate.Features.UpdateCandidateReference;
using GenclikMerkezi.Modules.Candidate.Features.UpdateCertificate;
using GenclikMerkezi.Modules.Candidate.Features.UpdateEducation;
using GenclikMerkezi.Modules.Candidate.Features.UpdateExperience;
using GenclikMerkezi.Modules.Candidate.Features.UploadCandidateCvFile;
using GenclikMerkezi.Modules.Candidate.Features.UploadCandidatePhoto;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Candidate;

public static class CandidateModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCandidateModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RegisterCandidateEndpoint.Map(app);

        // CandidateCv
        GetCandidateCvEndpoint.Map(app);
        UpdateCandidateCvContactInfoEndpoint.Map(app);
        UpdateCandidateCvPersonalInfoEndpoint.Map(app);
        UploadCandidatePhotoEndpoint.Map(app);
        ExportCandidateCvPdfEndpoint.Map(app);

        // CandidateCvContent
        GetCandidateCvContentEndpoint.Map(app);
        UpdateCandidateCvContentSummaryEndpoint.Map(app);
        UploadCandidateCvFileEndpoint.Map(app);

        // Deneyim
        AddExperienceEndpoint.Map(app);
        UpdateExperienceEndpoint.Map(app);
        RemoveExperienceEndpoint.Map(app);

        // Eğitim
        AddEducationEndpoint.Map(app);
        UpdateEducationEndpoint.Map(app);
        RemoveEducationEndpoint.Map(app);

        // Dil
        AddCandidateLanguageEndpoint.Map(app);
        UpdateCandidateLanguageEndpoint.Map(app);
        RemoveCandidateLanguageEndpoint.Map(app);

        // Sertifika
        AddCertificateEndpoint.Map(app);
        UpdateCertificateEndpoint.Map(app);
        RemoveCertificateEndpoint.Map(app);

        // Referans
        AddCandidateReferenceEndpoint.Map(app);
        UpdateCandidateReferenceEndpoint.Map(app);
        RemoveCandidateReferenceEndpoint.Map(app);

        // Admin/advisor tarafı için candidate listesi
        SearchCandidatesEndpoint.Map(app);
        AdminReassignCandidateEndpoint.Map(app);

        // Görüşme talebi (Görev 5/ADR-022 §4)
        RequestMeetingEndpoint.Map(app);
        ConfirmMeetingEndpoint.Map(app);

        // Toplu bildirim (Görev 8)
        SendBulkCandidateNotificationEndpoint.Map(app);

        return app;
    }
}
