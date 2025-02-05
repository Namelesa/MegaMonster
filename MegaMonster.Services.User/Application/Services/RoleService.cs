using MegaMonster.Services.User.Application.ResultOperation;
using MegaMonster.Services.User.Application.Validation.RoleValidator;
using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Services;

public class RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger, RoleValidation validator)
{
    public async Task<IEnumerable<Role>> GetAllRoles() => await roleRepository.GetAllAsync();

    public async Task<OperationResult> AddRole(Role role) =>
        await HandleDatabaseOperation(async () =>
        {
            if (string.IsNullOrWhiteSpace(role.RoleName))
                return OperationResult.Fail("Role name cannot be empty.");

            var validationResult = await validator.ValidateAsync(role);
            if (validationResult.Errors.Any())
                return OperationResult.Fail(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return await roleRepository.AddAsync(role)
                ? OperationResult.Ok()
                : OperationResult.Fail("Failed to add role.");
        }, $"Adding role '{role.RoleName}'");

    public async Task<OperationResult> EditRoleAsync(string oldName, string newName) =>
        await HandleDatabaseOperation(async () =>
        {
            if (string.IsNullOrWhiteSpace(newName) || newName.Length < 4)
                return OperationResult.Fail("New role name must be at least 4 characters.");

            var roles = await roleRepository.GetRolesByNamesAsync(oldName, newName);
            var currentRole = roles.FirstOrDefault(r => r.RoleName == oldName);
            var existingRole = roles.FirstOrDefault(r => r.RoleName == newName);

            if (currentRole is null)
                return OperationResult.Fail($"Role '{oldName}' not found.");

            if (existingRole is not null)
                return OperationResult.Fail($"Role '{newName}' already exists.");

            currentRole.RoleName = newName;
            return await roleRepository.EditAsync(currentRole)
                ? OperationResult.Ok()
                : OperationResult.Fail("Failed to update role.");
        }, $"Editing role '{oldName}' to '{newName}'");

    public async Task<OperationResult> DeleteRoleAsync(string name) =>
        await HandleDatabaseOperation(async () =>
        {
            if (string.IsNullOrWhiteSpace(name))
                return OperationResult.Fail("Role name cannot be empty.");

            var role = await roleRepository.FindRoleByNameAsync(name);
            if (role is null)
                return OperationResult.Fail($"Role '{name}' not found.");

            return await roleRepository.DeleteAsync(role)
                ? OperationResult.Ok()
                : OperationResult.Fail($"Failed to delete role '{name}'.");
        }, $"Deleting role '{name}'");

    public async Task<Role?> FindRoleByNameAsync(string name) =>
        string.IsNullOrWhiteSpace(name) ? null : await roleRepository.FindRoleByNameAsync(name);

    private async Task<OperationResult> HandleDatabaseOperation(Func<Task<OperationResult>> operation, string action)
    {
        try
        {
            var result = await operation();
            if (!result.Success)
                logger.LogWarning("{Action} failed: {Error}", action, result.Message);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Action} encountered an error.", action);
            return OperationResult.Fail("An unexpected error occurred.");
        }
    }
}
