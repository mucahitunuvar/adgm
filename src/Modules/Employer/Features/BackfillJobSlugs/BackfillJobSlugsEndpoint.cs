using GenclikMerkezi.BuildingBlocks.Infrastructure.Http;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Employer.Features.BackfillJobSlugs;

// Görev 1 (master prompt) "komutun nasıl tetiklendiğini raporda belirt" notu: repoda hiçbir modülde
// başlangıçta çalışan bir IHostedService/BackgroundService deseni yok (migration'lar da uygulamaya
// değil, dağıtım sırasında dotnet-ef ile uygulanıyor - EmployerDbContextFactory). Bu yüzden
// AdminReinstateJobEndpoint ile aynı admin-endpoint desenine uyularak, deploy sonrası admin tarafından
// bir kez tetiklenen bir uç noktaya karar verildi; tekrar çağrılması da zararsızdır (idempotent).
internal static class BackfillJobSlugsEndpoint
{
    private const string AdminRole = "Admin";

    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/api/v1/admin/jobs/backfill-slugs",
                async (ISender sender, CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(new BackfillJobSlugsCommand(), cancellationToken);
                    return result.ToOkOrProblem();
                })
            .RequireAuthorization(policy => policy.RequireRole(AdminRole))
            .RequireRateLimiting("authenticated")
            .WithName("BackfillJobSlugs")
            .WithTags("Employer");
    }
}
