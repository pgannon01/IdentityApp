using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace IdentityApp.Authorization
{
    public class InvoiceAdminAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Invoice>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Invoice resource)
        {
            if (context.User == null || resource == null)
                return Task.CompletedTask;

            // Don't need to check to see if the Admin is trying to do certain actions, because the Admin should be allowed to do everything

            if (context.User.IsInRole(Constants.InvoiceAdminRole))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
