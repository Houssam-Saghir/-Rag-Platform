using System.Security.Claims;
using Rag.Application.Exceptions;

namespace Rag.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : throw new UnauthorizedException("Invalid or missing user id claim");
    }
}
