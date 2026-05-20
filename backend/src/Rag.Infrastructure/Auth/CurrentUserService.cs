using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Rag.Application.Interfaces;

namespace Rag.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
            var value = accessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserEmail => accessor.HttpContext?.User.FindFirst(ClaimTypes.Email)?.Value;
}
