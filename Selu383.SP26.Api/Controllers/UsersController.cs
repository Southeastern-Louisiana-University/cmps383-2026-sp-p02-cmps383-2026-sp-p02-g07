using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Selu383.SP26.Api.Features.Roles;
using Selu383.SP26.Api.Features.Users;

namespace Selu383.SP26.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController(
    UserManager<User> userManager,
    RoleManager<Role> roleManager
) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.UserName))
        {
            return BadRequest();
        }
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest();
        }
        if (dto.Roles == null || dto.Roles.Length == 0)
        {
            return BadRequest();
        }

        var duplicate = await userManager.FindByNameAsync(dto.UserName);
        if (duplicate != null)
        {
            return BadRequest();
        }

        // validate roles
        foreach (var roleName in dto.Roles)
        {
            if (string.IsNullOrWhiteSpace(roleName) || !await roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest();
            }
        }

        var user = new User { UserName = dto.UserName };
        var createResult = await userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            return BadRequest();
        }

        var roleResult = await userManager.AddToRolesAsync(user, dto.Roles);
        if (!roleResult.Succeeded)
        {
            return BadRequest();
        }

        var roles = await userManager.GetRolesAsync(user);
        var result = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Roles = roles.ToArray(),
        };
        return Ok(result);
    }
}
