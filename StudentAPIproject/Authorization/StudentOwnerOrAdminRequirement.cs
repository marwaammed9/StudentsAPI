using Microsoft.AspNetCore.Authorization;


namespace StudentAPIproject.Authorization
{
    public class StudentOwnerOrAdminRequirement : IAuthorizationRequirement
    {
    }
}
