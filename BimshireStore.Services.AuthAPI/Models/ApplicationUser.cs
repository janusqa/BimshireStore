using Microsoft.AspNetCore.Identity;

namespace BimshireStore.Services.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
    }
}