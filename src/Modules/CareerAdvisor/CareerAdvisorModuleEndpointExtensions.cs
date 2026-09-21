using GenclikMerkezi.Modules.CareerAdvisor.Features.AddCandidateNote;
using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using GenclikMerkezi.Modules.CareerAdvisor.Features.GetCandidateNotes;
using GenclikMerkezi.Modules.CareerAdvisor.Features.GetGeneralPool;
using GenclikMerkezi.Modules.CareerAdvisor.Features.ProposeMeetingTime;
using GenclikMerkezi.Modules.CareerAdvisor.Features.RejectMeetingRequest;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor;

public static class CareerAdvisorModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCareerAdvisorModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateCareerAdvisorEndpoint.Map(app);
        AddCandidateNoteEndpoint.Map(app);
        GetCandidateNotesEndpoint.Map(app);
        ProposeMeetingTimeEndpoint.Map(app);
        RejectMeetingRequestEndpoint.Map(app);
        GetGeneralPoolEndpoint.Map(app);

        // DeactivateCareerAdvisorCommand'ın kendisi doğrudan bir HTTP endpoint'e bağlı değil (Görev 3/
        // ADR-022 §1): yeniden atamasız çıplak bir deaktivasyon, danışmana atanmış adayları "öksüz"
        // bırakırdı. Tek genel-erişimli deaktivasyon endpoint'i Host katmanındaki
        // AdminCareerAdvisorEndpoints.MapAdminCareerAdvisorEndpoints() - deactivate + reassign'ı
        // sırayla orkestre eder.
        //
        // CreateMeetingRequestCommand/ConfirmMeetingRequestCommand de endpoint'siz (Görev 5/ADR-022
        // §4): aday tarafındaki akış Candidate → CareerAdvisor yönünü kullanır (Candidate modülünün
        // RequestMeetingCommand/ConfirmMeetingCommand'ı ICareerAdvisorModuleContract üzerinden çağırır).

        return app;
    }
}
