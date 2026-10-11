using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SalonFryzjerski.Models;
using SalonFryzjerski.Services;

namespace SalonFryzjerski.Security;

public class RequireModuleAccessAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly AppModule _module;
    private readonly AccessLevel _required;

    public RequireModuleAccessAttribute(AppModule module, AccessLevel required)
    {
        _module = module;
        _required = required;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var access = context.HttpContext.RequestServices.GetRequiredService<IAccessService>();
        var ok = await access.HasAccessAsync(context.HttpContext.User, _module, _required);

        if (!ok) context.Result = new ForbidResult();
    }
}
