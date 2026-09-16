using GenclikMerkezi.Modules.Identity.Features.AdminGetUserById;
using GenclikMerkezi.Modules.Identity.Features.AdminGetUsers;
using GenclikMerkezi.Modules.Identity.Features.AdminUnlockUser;
using GenclikMerkezi.Modules.Identity.Features.ChangePassword;
using GenclikMerkezi.Modules.Identity.Features.ChangeUserRole;
using GenclikMerkezi.Modules.Identity.Features.ForgotPassword;
using GenclikMerkezi.Modules.Identity.Features.GetCurrentUser;
using GenclikMerkezi.Modules.Identity.Features.Login;
using GenclikMerkezi.Modules.Identity.Features.Logout;
using GenclikMerkezi.Modules.Identity.Features.RefreshAccessToken;
using GenclikMerkezi.Modules.Identity.Features.RegisterUser;
using GenclikMerkezi.Modules.Identity.Features.ResendVerificationEmail;
using GenclikMerkezi.Modules.Identity.Features.ResetPassword;
using GenclikMerkezi.Modules.Identity.Features.VerifyEmail;
using Microsoft.AspNetCore.Routing;

namespace GenclikMerkezi.Modules.Identity;

public static class IdentityModuleEndpointExtensions
{
    public static IEndpointRouteBuilder MapIdentityModuleEndpoints(this IEndpointRouteBuilder app)
    {
        RegisterUserEndpoint.Map(app);
        LoginEndpoint.Map(app);
        RefreshAccessTokenEndpoint.Map(app);
        LogoutEndpoint.Map(app);
        GetCurrentUserEndpoint.Map(app);
        ChangePasswordEndpoint.Map(app);
        ForgotPasswordEndpoint.Map(app);
        ResetPasswordEndpoint.Map(app);
        ChangeUserRoleEndpoint.Map(app);
        VerifyEmailEndpoint.Map(app);
        ResendVerificationEmailEndpoint.Map(app);
        AdminGetUsersEndpoint.Map(app);
        AdminGetUserByIdEndpoint.Map(app);
        AdminUnlockUserEndpoint.Map(app);

        return app;
    }
}
