using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Data.Models;

public class ApplicationRole : IdentityRole<Guid> {
    public virtual ICollection<ApplicationUserRole> UserRoles { get; } = [];
}
