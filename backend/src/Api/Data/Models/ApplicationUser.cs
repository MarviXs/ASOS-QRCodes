using Microsoft.AspNetCore.Identity;

namespace Fei.Is.Api.Data.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public virtual ICollection<ApplicationUserRole>? UserRoles { get; set; }
    public virtual DateTimeOffset RegistrationDate { get; set; }
}
