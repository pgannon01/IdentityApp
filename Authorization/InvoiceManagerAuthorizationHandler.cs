using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace IdentityApp.Authorization
{
    public class InvoiceManagerAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Invoice>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Invoice resource)
        {
            if (context.User == null || resource == null)
                return Task.CompletedTask;

            if (requirement.Name != Constants.ApprovedOperationName && // Managers will only be approving or rejecting invoices
                requirement.Name != Constants.RejectedOperationName)
            {
                return Task.CompletedTask; // If not approving or rejecting, not authorized
            }

            if (context.User.IsInRole(Constants.InvoiceManagersRole))
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
