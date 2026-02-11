using Microsoft.AspNetCore.Identity;

namespace Selu383.SP26.Api.Features.Roles;

public class Role : IdentityRole<int>
{
    public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}