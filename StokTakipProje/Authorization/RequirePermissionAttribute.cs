using Microsoft.AspNetCore.Authorization;

namespace StokTakipProje.Authorization;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permissionCode)
    {
        Policy = $"Permission:{permissionCode}";
    }
}