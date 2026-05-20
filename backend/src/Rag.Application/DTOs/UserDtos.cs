namespace Rag.Application.DTOs;

public record UserDto(Guid Id, string Email, string FirstName, string LastName, List<string> Roles, bool IsActive);
public record CreateUserRequest(string Email, string Password, string FirstName, string LastName, string Role);
public record UpdateUserRequest(string FirstName, string LastName, bool IsActive);
public record AssignRoleRequest(string Role);
