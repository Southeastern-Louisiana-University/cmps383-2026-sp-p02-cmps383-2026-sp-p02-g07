using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Users;

public class User : IdentityUser<int>
{
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
