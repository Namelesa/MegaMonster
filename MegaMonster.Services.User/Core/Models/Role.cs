namespace MegaMonster.Services.User.Core.Models;

public class Role
{
    public Guid Id { get; set; }
    public string RoleName { get; set; }
    public Role(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name cannot be empty.");
        }

        if (roleName.Length < 4)
        {
            throw new ArgumentException("Role name must be at least 4 characters long.");
        }
        
        RoleName = roleName;
    }
}