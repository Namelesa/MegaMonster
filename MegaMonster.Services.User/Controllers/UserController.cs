using System.ComponentModel.DataAnnotations;
using MegaMonster.Services.User.Data;
using MegaMonster.Services.User.Dto_s;
using MegaMonster.Services.User.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MegaMonster.Services.User.Controllers;
[ApiController]
[Route("api/users")]
public class UserController(AppDbContext db) : ControllerBase
{
    // Get Requests //
    [HttpGet("getAllRoles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await db.Roles.ToArrayAsync();
        return Ok(roles);
    }
    
    [HttpGet("getAllUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await db.Users.ToArrayAsync();
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
        try
        {
            db.Roles.Add(role);
            await db.SaveChangesAsync();
            return Ok("Add new role");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return BadRequest("Error with adding");
        }
    }
    
    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody, Required] UserDto userDto, [Required] string role)
    {
        var currentRole = db.Roles.FirstOrDefault(r=> r.RoleName == role);
        
        Users user = new Users();
        if (currentRole != null)
        {
            user.Login = userDto.Login;
            user.UserName = userDto.UserName;
            user.NormalizedUserName = userDto.UserName.ToUpper();
            user.NormalizedEmail = userDto.Email.ToUpper();
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Role = currentRole;
            user.RoleId = currentRole.Id;
            user.PasswordHash = userDto.PasswordHash;
        }
        else
        {
            return NotFound("Not found this role");
        }
        
        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Ok("Add user");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Error with adding user");
        }
    }
    
    // Put Requests // 
    [HttpPut("editRole")]
    public async Task<IActionResult> EditRole([Required] string oldName, [Required] string newName)
    {
        var currentRole = db.Roles.FirstOrDefault(r=> r.RoleName == oldName);
        if (currentRole != null)
        {
            try
            {
                currentRole.RoleName = newName;
                db.Roles.Update(currentRole);
                await db.SaveChangesAsync();
                return Ok("Role update");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest("Error with updating");
            }
        }
        else
        {
            return BadRequest("Role does not updated");
        }
    }
    
    [HttpPut("editUser")]
    public async Task<IActionResult> EditUser([Required, FromBody] UserEditDto userEditDto, [Required] string login)
    {
        var currentUser = db.Users.FirstOrDefault(u => u.Login == login);
        if (currentUser == null)
        {
            return NotFound("Not found user");
        }
        
        currentUser.Login = userEditDto.Login;
        currentUser.UserName = userEditDto.UserName;
        currentUser.NormalizedUserName = userEditDto.UserName.ToUpper();
        currentUser.NormalizedEmail = userEditDto.Email.ToUpper();
        currentUser.Email = userEditDto.Email;
        currentUser.PhoneNumber = userEditDto.PhoneNumber;

        try
        {
            db.Users.Update(currentUser);
            await db.SaveChangesAsync();
            return Ok("User update");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Error with update");
        }
    }
    
    // Delete Requests //
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteRole([Required] string name)
    {
        var currentRole = db.Roles.FirstOrDefault(r=> r.RoleName == name);
        if (currentRole != null)
        {
            db.Roles.Remove(currentRole);
            await db.SaveChangesAsync();
            return Ok("Delete role is ok");
        }
        
        return NotFound("Role not found");
    }
    
    [HttpDelete("ban")]
    public async Task<IActionResult> DeleteUser([Required] string login)
    {
        var currentUser = db.Users.FirstOrDefault(u=> u.Login == login);
        if (currentUser != null)
        {
            db.Users.Remove(currentUser);
            await db.SaveChangesAsync();
            return Ok("User was banned");
        }
        
        return NotFound("User not found");
    }
    
    // Admin //
    [HttpPost("createAdmin")]
    public async Task<IActionResult> AddUserAdmin([FromBody, Required] UserDto userDto)
    {
        var currentRole = db.Roles.FirstOrDefault(r=> r.RoleName == "Admin");
        
        Users user = new Users();
        if (currentRole != null)
        {
            user.Login = userDto.Login;
            user.UserName = userDto.UserName;
            user.NormalizedUserName = userDto.UserName.ToUpper();
            user.NormalizedEmail = userDto.Email.ToUpper();
            user.Email = userDto.Email;
            user.PhoneNumber = userDto.PhoneNumber;
            user.Role = currentRole;
            user.RoleId = currentRole.Id;
            user.PasswordHash = userDto.PasswordHash;
        }
        else
        {
            return NotFound("Not found this role");
        }
        
        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Ok("Add admin");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return BadRequest("Error with adding admin");
        }
    }
    
    
}