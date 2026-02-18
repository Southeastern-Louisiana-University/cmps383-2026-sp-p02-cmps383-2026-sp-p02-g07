using Microsoft.AspNetCore.Identity;
using Selu383.SP26.Api.Features.Roles;

namespace Selu383.SP26.Api.Features.Users;

public class UserRole : IdentityUserRole<int>
{
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;
}
