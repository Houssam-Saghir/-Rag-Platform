using Microsoft.EntityFrameworkCore;
using Rag.Application.DTOs;
using Rag.Application.Exceptions;
using Rag.Application.Interfaces;
using Rag.Domain.Entities;
using Rag.Infrastructure.Persistence;

namespace Rag.Infrastructure.Services;

public class UserManagementService(ApplicationDbContext dbContext) : IUserManagementService
{
    public async Task<IReadOnlyCollection<UserDto>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await dbContext.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).ToListAsync(ct);
        return users.Select(Map).ToList();
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await dbContext.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("User not found");
        return Map(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        if (await dbContext.Users.AnyAsync(x => x.Email == request.Email, ct))
            throw new ValidationException("Email already exists");

        var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == request.Role, ct)
            ?? throw new NotFoundException("Role not found");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true
        };

        dbContext.Users.Add(user);
        dbContext.UserRoles.Add(new UserRole { User = user, Role = role });
        await dbContext.SaveChangesAsync(ct);

        return await GetByIdAsync(user.Id, ct);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await dbContext.Users.Include(x => x.UserRoles).ThenInclude(x => x.Role).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("User not found");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(ct);
        return Map(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("User not found");

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task AssignRoleAsync(Guid id, string role, CancellationToken ct = default)
    {
        var user = await dbContext.Users.Include(x => x.UserRoles).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("User not found");

        var roleEntity = await dbContext.Roles.FirstOrDefaultAsync(x => x.Name == role, ct)
            ?? throw new NotFoundException("Role not found");

        if (user.UserRoles.All(x => x.RoleId != roleEntity.Id))
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleEntity.Id });
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static UserDto Map(User u) => new(u.Id, u.Email, u.FirstName, u.LastName, u.UserRoles.Select(x => x.Role.Name).ToList(), u.IsActive);
}
