using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LastGrind.Application.Authorization
{
    public class MustHaveCompanyDomainHandler : AuthorizationHandler<MustHaveCompanyDomainRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MustHaveCompanyDomainRequirement requirement)
        {
            var userEmailAddress = context.User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            if (userEmailAddress.EndsWith(requirement.DomainName))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
            context.Fail();
            return Task.CompletedTask;
        }
    }
}
