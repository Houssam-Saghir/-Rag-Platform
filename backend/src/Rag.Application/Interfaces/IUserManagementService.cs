using Rag.Application.DTOs;

namespace Rag.Application.Interfaces;

public interface IUserManagementService
{
    Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task AssignRoleAsync(Guid id, string role, CancellationToken ct = default);
}
