using Microsoft.AspNetCore.Identity;
namespace LastGrind.Domain.Entities
{
    public class Post:BaseEntity
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
