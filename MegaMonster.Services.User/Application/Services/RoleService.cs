using MegaMonster.Services.User.Application.ResultOperation;
using MegaMonster.Services.User.Application.Validation.RoleValidator;
using MegaMonster.Services.User.Core.Interfaces;
using MegaMonster.Services.User.Core.Models;

namespace MegaMonster.Services.User.Application.Services;

public class RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger)
{
    public async Task<IEnumerable<Role>> GetAllRoles()
    {
        return await roleRepository.GetAllAsync();
    }
    public async Task<OperationResult> AddRole(Role role)
    {
        if (string.IsNullOrWhiteSpace(role.RoleName))
        {
            return OperationResult.Fail("New role name cannot be empty.");
        }

        var validator = new RoleValidation();
        var validationResult = await validator.ValidateAsync(role);
        if (validationResult.Errors.Any())
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            return OperationResult.Fail(errors);
        }
        bool result = await roleRepository.AddAsync(role);
        return result ? OperationResult.Ok() : OperationResult.Fail("Failed to add role.");
        
    }
    public async Task<OperationResult> EditRoleAsync(string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) && newName.Length < 4)
        {
            return OperationResult.Fail("New role name cannot be empty.");
        }
        var roles = await roleRepository.GetRolesByNamesAsync(oldName, newName);
        var currentRole = roles.FirstOrDefault(r => r.RoleName == oldName);
        var existingRole = roles.FirstOrDefault(r => r.RoleName == newName);
        
        if (currentRole == null)
        {
            logger.LogWarning("Role '{OldName}' not found.", oldName);
            return OperationResult.Fail($"Role '{oldName}' not found.");
        }
        
        if (existingRole != null)
        {
            return OperationResult.Fail($"Role '{newName}' already exists.");
        }
        
        currentRole.RoleName = newName;
        bool updated = await roleRepository.EditAsync(currentRole);
        return updated ? OperationResult.Ok() : OperationResult.Fail("Failed to update role.");
    }
    public async Task<OperationResult> DeleteRoleAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return OperationResult.Fail("Role name cannot be empty.");
        }

        var role = await roleRepository.FindRoleByNameAsync(name);
        if (role == null)
        {
            return OperationResult.Fail($"Role '{name}' not found.");
        }

        var result = await roleRepository.DeleteAsync(role);
        return result 
            ? OperationResult.Ok() 
            : OperationResult.Fail($"Failed to delete role '{name}'.");
    }
    public async Task<Role?> FindRoleByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        return await roleRepository.FindRoleByNameAsync(name);
    }
}