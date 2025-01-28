using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.User.Application.Interfaces;
using MegaMonster.Services.User.Core.Models;
using MegaMonster.Services.User.WebApi.Dto_s;
using Microsoft.AspNetCore.Mvc;

namespace MegaMonster.Services.User.WebApi.Controllers;
[ApiController]
[Route("api/users")]
public class UserController(IRoleRepository roleRepository, IUserRepository userRepository) : ControllerBase
{
    // Get Requests //
    [HttpGet("getAllRoles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await roleRepository.GetAllAsync();
        return Ok(roles);
    }
    
    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await userRepository.GetAllAsync();
        return Ok(users);
    }
    
    // Post Requests //
    [HttpPost("addRole")]
    public async Task<IActionResult> AddRole([FromBody, Required] RoleDto roleDto)
    {
        Role role = new Role()
        {
            RoleName = roleDto.RoleName
        };
        bool result = await roleRepository.AddAsync(role);
        return result ? Ok("Add new role") : BadRequest("Error with adding");
    }
    
    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody, Required] UserDto userDto, [Required] string role)
    {
        var currentRole = await roleRepository.FindRoleByNameAsync(role);
        if (currentRole == null) return NotFound("Not found this role");

        Users user = new Users()
        {
            Login = userDto.Login,
            UserName = userDto.UserName,
            NormalizedUserName = userDto.UserName.ToUpper(),
            NormalizedEmail = userDto.Email.ToUpper(),
            Email = userDto.Email,
            PhoneNumber = userDto.PhoneNumber,
            Role = currentRole,
            RoleId = currentRole.Id,
            PasswordHash = userDto.PasswordHash
        };
        
        var result = await userRepository.AddAsync(user);
        return result ? Ok("Add user") : BadRequest("Error with adding user");
    }
    
    // Put Requests // 
    [HttpPut("editRole")]
    public async Task<IActionResult> EditRole([Required] string oldName, [Required] string newName)
    {
        var currentRole = await roleRepository.FindRoleByNameAsync(oldName);
        if (currentRole == null) return NotFound("Role does not founded");
        
        currentRole.RoleName = newName;
        bool result = await roleRepository.EditAsync(currentRole);
        return result ? Ok("Role update") : BadRequest("Error with updating");
    }
    
    [HttpPut("editUser")]
    public async Task<IActionResult> EditUser([Required, FromBody] UserEditDto userEditDto, [Required] string login)
    {
        var currentUser = await userRepository.GetUserByLoginAsync(login);
        if (currentUser == null) return NotFound("Not found user");
        
        currentUser.Login = userEditDto.Login;
        currentUser.UserName = userEditDto.UserName;
        currentUser.NormalizedUserName = userEditDto.UserName.ToUpper();
        currentUser.NormalizedEmail = userEditDto.Email.ToUpper();
        currentUser.Email = userEditDto.Email;
        currentUser.PhoneNumber = userEditDto.PhoneNumber;

        bool result = await userRepository.EditAsync(currentUser);
        return result ? Ok("User update") : BadRequest("Error with update");
    }
    
    // Delete Requests //
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteRole([Required] string name)
    {
        var currentRole = await roleRepository.FindRoleByNameAsync(name);
        if (currentRole == null) return NotFound("Role not found");
        
        bool result = await roleRepository.DeleteAsync(currentRole);
        return result ? Ok("Delete role is ok") : BadRequest("Error with deleted role");
    }
    
    [HttpDelete("ban")]
    public async Task<IActionResult> DeleteUser([Required] string login)
    {
        var currentUser = await userRepository.GetUserByLoginAsync(login);
        if (currentUser == null) return NotFound("User not found");
        
        bool result = await userRepository.DeleteAsync(currentUser);
        return result ? Ok("User was banned") : BadRequest("Error with baning this user");
    }
    
    // Admin //
    [HttpPost("createAdmin")]
    public async Task<IActionResult> AddUserAdmin([FromBody, Required] UserDto userDto, string role = "Admin")
    {
        var currentRole = await roleRepository.FindRoleByNameAsync(role);
        
        if (currentRole == null) return NotFound("Not found this role");
        Users user = new Users()
        {
            Login = userDto.Login,
            UserName = userDto.UserName,
            NormalizedUserName = userDto.UserName.ToUpper(),
            NormalizedEmail = userDto.Email.ToUpper(),
            Email = userDto.Email,
            PhoneNumber = userDto.PhoneNumber,
            Role = currentRole,
            RoleId = currentRole.Id,
            PasswordHash = userDto.PasswordHash
        };

        bool result = await userRepository.AddAsync(user);
        return result ? Ok("Add new admin") : BadRequest("Error with adding admin");
    }
}