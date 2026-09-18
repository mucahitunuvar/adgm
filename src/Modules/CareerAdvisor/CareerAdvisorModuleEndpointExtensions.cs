using GenclikMerkezi.Modules.CareerAdvisor.Features.CreateCareerAdvisor;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.CareerAdvisor;

public static class CareerAdvisorModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapCareerAdvisorModuleEndpoints(this IEndpointRouteBuilder app)
    {
        CreateCareerAdvisorEndpoint.Map(app);

        // DeactivateCareerAdvisorCommand'ın kendisi doğrudan bir HTTP endpoint'e bağlı değil (Görev 3/
        // ADR-022 §1): yeniden atamasız çıplak bir deaktivasyon, danışmana atanmış adayları "öksüz"
        // bırakırdı. Tek genel-erişimli deaktivasyon endpoint'i Host katmanındaki
        // AdminCareerAdvisorEndpoints.MapAdminCareerAdvisorEndpoints() - deactivate + reassign'ı
        // sırayla orkestre eder.

        return app;
    }
}
