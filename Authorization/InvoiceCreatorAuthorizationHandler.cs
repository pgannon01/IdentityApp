using Microsoft.AspNetCore.Identity;
using IdentityApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace IdentityApp.Authorization
{
    public class InvoiceCreatorAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, Invoice>
    {
        UserManager<IdentityUser> _userManager;

        // Cheacks if the user is the creator of the given invoice
        public InvoiceCreatorAuthorizationHandler(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationAuthorizationRequirement requirement, Invoice resource)
        {
            if (context.User == null || resource == null)
                return Task.CompletedTask; // If something goes wrong

            if (requirement.Name != Constants.CreateOperationName && // Check no CRUD operation going on
                requirement.Name != Constants.ReadOperationName &&
                requirement.Name != Constants.UpdateOperationName &&
                requirement.Name != Constants.DeleteOperationName)
            {
                return Task.CompletedTask; // Not trying to preform a CRUD operation, return
            }

            // If none of the above, then that means the user is trying to do CRUD
            if (resource.CreatorId == _userManager.GetUserId(context.User))
            {
                context.Succeed(requirement); // If authorization is working, continue 

            }

            return Task.CompletedTask;
        }
    }
}
