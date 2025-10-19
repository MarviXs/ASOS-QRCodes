using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Data.Models;

public class ApplicationUserRole : IdentityUserRole<Guid>
{
    public virtual ApplicationUser? User { get; set; }
    public virtual ApplicationRole? Role { get; set; }
}

