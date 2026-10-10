using Microsoft.AspNetCore.Authorization;
using StudentAPIproject.Authorization;
using System.Security.Claims;

// Same conceptual rule as StudentOwnerOrAdminHandler,
// but the resource here is the student's email instead of the numeric id.
public class StudentOwnerOrAdminByEmailHandler
    : AuthorizationHandler<StudentOwnerOrAdminRequirement, string>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StudentOwnerOrAdminRequirement requirement,
        string email)
    {
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userEmail = context.User.FindFirstValue(ClaimTypes.Email);

        if (!string.IsNullOrEmpty(userEmail) &&
            string.Equals(userEmail, email, StringComparison.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}