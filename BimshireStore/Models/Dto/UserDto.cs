using System.ComponentModel.DataAnnotations;

namespace BimshireStore.Models.Dto
{

    public record UserDto
    {
        public required string Id { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public record UserRegisterRequestDto
    {
        [Required]
        public required string UserName { get; set; }
        [Required]
        public string? Name { get; set; }
        [Required]
        public string? PhoneNumber { get; set; }
        [Required]
        public required string Password { get; set; }
        public string? Role { get; set; }
    }

    public record UserLoginRequestDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public record UserAuthResponseDto
    {
        [Required]
        public required UserDto User { get; set; }
        [Required]
        public required string Token { get; set; }
    }

}