using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rag.Application.DTOs;
using Rag.Application.Interfaces;

namespace Rag.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetUsers(CancellationToken ct)
        => Ok(await userManagementService.GetAllAsync(ct));

    [HttpGet("users/{id:guid}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id, CancellationToken ct)
        => Ok(await userManagementService.GetByIdAsync(id, ct));

    [HttpPost("users")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserRequest request, CancellationToken ct)
        => Ok(await userManagementService.CreateAsync(request, ct));

    [HttpPut("users/{id:guid}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserRequest request, CancellationToken ct)
        => Ok(await userManagementService.UpdateAsync(id, request, ct));

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await userManagementService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("users/{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, AssignRoleRequest request, CancellationToken ct)
    {
        await userManagementService.AssignRoleAsync(id, request.Role, ct);
        return NoContent();
    }
}
