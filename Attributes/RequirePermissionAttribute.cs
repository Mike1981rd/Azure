using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace WebsiteBuilderAPI.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _permission;

        // Constructor que acepta un solo parámetro (backward compatibility)
        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        // Constructor que acepta resource y action (nuevo)
        public RequirePermissionAttribute(string resource, string action)
        {
            _permission = $"{resource}.{action}";
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Verificar si el usuario está autenticado
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Verificar si es SuperAdmin (bypass todos los permisos)
            var roles = context.HttpContext.User.FindAll(ClaimTypes.Role).Select(c => c.Value);
            if (roles.Contains("SuperAdmin"))
            {
                return; // SuperAdmin tiene acceso a todo
            }

            // Verificar permisos específicos
            var permissions = context.HttpContext.User.FindAll("permissions").Select(c => c.Value);
            
            if (!permissions.Contains(_permission))
            {
                context.Result = new ForbidResult($"Permission '{_permission}' is required.");
            }
        }
    }
}