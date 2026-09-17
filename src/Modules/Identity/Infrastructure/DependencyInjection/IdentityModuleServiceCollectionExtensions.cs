using System.Text;
using GenclikMerkezi.Contracts.Identity;
using GenclikMerkezi.Modules.Identity;
using GenclikMerkezi.Modules.Identity.Application.Abstractions;
using GenclikMerkezi.Modules.Identity.Infrastructure.Persistence;
using GenclikMerkezi.Modules.Identity.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GenclikMerkezi.Modules.Identity.Infrastructure.DependencyInjection;

public static class IdentityModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt configuration section is missing.");

        services.Configure<PasswordResetSettings>(configuration.GetSection(PasswordResetSettings.SectionName));
        services.Configure<EmailVerificationSettings>(configuration.GetSection(EmailVerificationSettings.SectionName));

        services.AddDbContext<IdentityDbContext>(options =>
        {
            // Always SqlServer, in every environment: IdentityDbContext is the CAP transactional
            // outbox anchor (AddMessaging<IdentityDbContext>() at the host composition root - CAP
            // only supports a single instance per process, see ADR-014's amendment), and CAP's
            // SqlServer storage package cannot target a Sqlite connection. Integration tests use a
            // real (throwaway, per-run) LocalDB database instead of ADR-012's usual Sqlite switch.
            var connectionString = configuration.GetConnectionString("IdentityDatabase");
            options.UseSqlServer(connectionString);
        });

        services.AddKeyedScoped<SharedKernel.Abstractions.IUnitOfWork>(
            IdentityModuleMarker.UnitOfWorkKey,
            (sp, _) => sp.GetRequiredService<IdentityDbContext>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IPasswordResetTokenGenerator, PasswordResetTokenGenerator>();
        services.AddScoped<IEmailVerificationTokenGenerator, EmailVerificationTokenGenerator>();

        // Published Contracts interface (ADR-016 Decision 2, Option C) - the in-process read path
        // other modules (e.g. Candidate's registration orchestration) use instead of IdentityDbContext.
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        services.AddAuthorization();

        return services;
    }
}
