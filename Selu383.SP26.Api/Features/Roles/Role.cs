using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Roles;

public class Role : IdentityRole<int>
{
    public ICollection<Users.UserRole> UserRoles { get; set; } = new List<Users.UserRole>();
}
