using Microsoft.AspNetCore.Identity;

namespace GsC.API.Models
{
    public class UserRole : IdentityUserRole<int>
    {
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        public int? AssignedBy { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
    }
}