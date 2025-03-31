using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.User.Application.Services;
using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.WebApi.Dto_s;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.User.WebApi.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(UserService userService, RoleService roleService) : ControllerBase
{
    // Get Requests
    [Authorize(Roles = "Admin")]
    [HttpGet("getAllRoles")]
    public async Task<IActionResult> GetRoles() => Ok(await roleService.GetAllRoles());
    
    [Authorize(Roles = "Admin")]
    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetUsers() => Ok(await userService.GetAllUsers());
    
    // Post Requests
    [Authorize(Roles = "Admin")]
    [HttpPost("addRole")]
    public async Task<IActionResult> AddRole([FromBody, Required] RoleDto roleDto)
    {
        if (string.IsNullOrWhiteSpace(roleDto.RoleName)) 
            return BadRequest(new { error = "Role name cannot be empty." });
        
        Role role = new Role(roleDto.RoleName);
        var result = await roleService.AddRole(role);
        
        return result.Success ? Ok(new { message = "Role is added" }) : BadRequest(new { error = result.Message });
    }
    
    [Authorize]
    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody, Required] UserDto userDto, [Required] string role)
    {
        var currentRole = await roleService.FindRoleByNameAsync(role);
        if (currentRole == null) return NotFound("Role not found.");
        
        Users user = new Users(userDto.Login)
        {
            UserName = userDto.UserName,
            NormalizedUserName = userDto.UserName.ToUpper(),
            NormalizedEmail = userDto.Email.ToUpper(),
            Email = userDto.Email,
            PhoneNumber = userDto.PhoneNumber,
            Role = currentRole,
            RoleId = currentRole.Id,
            PasswordHash = userDto.PasswordHash
        };
        
        var result = await userService.AddUser(user, role);
        return result.Success ? Ok("Register added.") : BadRequest(new { error = result.Message });
    }
    
    // Put Requests 
    [Authorize(Roles = "Admin")]
    [HttpPut("editRole")]
    public async Task<IActionResult> EditRole([Required] string oldName, [Required] string newName)
    {
        var result = await roleService.EditRoleAsync(oldName, newName);
        return result.Success ? Ok(new { message = "Role is edited" }) : BadRequest(new { error = result.Message });
    }
    
    [Authorize]
    [HttpPut("editUser")]
    public async Task<IActionResult> EditUser([Required, FromBody] UserEditDto userEditDto, [Required] string login)
    {
        var result = await userService.EditUser(login, userEditDto.UserName, userEditDto.Email, userEditDto.PhoneNumber, userEditDto.Login);
        return result.Success ? Ok("Register updated") : BadRequest(new { error = result.Message });
    }
    
    // Delete Requests
    [Authorize(Roles = "Admin")]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteRole([Required] string name)
    {
        var result = await roleService.DeleteRoleAsync(name);
        return result.Success ? Ok(new { message = "Role deleted" }) : BadRequest(new { error = result.Message });
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("ban")]
    public async Task<IActionResult> DeleteUser([Required] string login, [Required] string reason)
    {
        var result = await userService.DeleteUser(login, reason);
        return result.Success ? Ok("Register banned") : BadRequest(new { error = result.Message });
    }
    
    // Admin
    [Authorize(Roles = "Admin")]
    [HttpPost("createAdmin")]
    public async Task<IActionResult> AddUserAdmin([FromBody, Required] UserDto userDto, string role = "Admin")
    {
        var currentRole = await roleService.FindRoleByNameAsync(role);
        if (currentRole == null) return NotFound("Role not found.");
        
        Users user = new Users(userDto.Login)
        {
            UserName = userDto.UserName,
            NormalizedUserName = userDto.UserName.ToUpper(),
            NormalizedEmail = userDto.Email.ToUpper(),
            Email = userDto.Email,
            PhoneNumber = userDto.PhoneNumber,
            Role = currentRole,
            RoleId = currentRole.Id,
            PasswordHash = userDto.PasswordHash
        };
        
        var result = await userService.AddUser(user, role);
        return result.Success ? Ok("New admin added") : BadRequest(new { error = result.Message });
    }
}
