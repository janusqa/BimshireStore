using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BimshireStore.Services.AuthAPI.Data;
using BimshireStore.Services.AuthAPI.Models;
using BimshireStore.Services.AuthAPI.Models.Dto;
using BimshireStore.Services.AuthAPI.Models.Extensions;
using BimshireStore.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BimshireStore.Services.AuthAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;
        private readonly RoleManager<IdentityRole> _rm;
        private readonly SignInManager<ApplicationUser> _sm;
        private readonly IdentityService _ident;

        public AuthService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> um,
            RoleManager<IdentityRole> rm,
            SignInManager<ApplicationUser> sm,
            IdentityService ident
        )
        {
            _db = db;
            _um = um;
            _rm = rm;
            _sm = sm;
            _ident = ident;
        }

        public async Task<bool> AssignRole(string userName, string role)
        {
            var user = await _um.FindByNameAsync(userName);
            if (user is null) return false;

            if (!(await _rm.RoleExistsAsync(role)))
            {
                var roleCreated = await _rm.CreateAsync(new IdentityRole(role));
                if (!roleCreated.Succeeded) return false;
            }

            var userAssignedToRole = await _um.AddToRoleAsync(user, role);
            if (!userAssignedToRole.Succeeded) return false;

            return true;
        }

        public async Task<UserAuthResponseDto> Login(UserLoginRequestDto request)
        {
            // 1. Chat that user exists
            var user = await _um.FindByNameAsync(request.UserName);
            if (user is null)
            {
                return new UserAuthResponseDto
                {
                    User = new UserDto { Id = string.Empty, Email = string.Empty, UserName = string.Empty },
                    Token = string.Empty
                };
            }

            // 2. Authenticate user
            var result = await _sm.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                return new UserAuthResponseDto
                {
                    User = new UserDto { Id = string.Empty, Email = string.Empty, UserName = string.Empty },
                    Token = string.Empty
                };
            }

            // 3. Get user Roles
            var roles = await _um.GetRolesAsync(user);

            // 4. Create User Claims
            var userClaims = new List<Claim>();
            foreach (var role in roles)
            {
                userClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // 5. Create claims identity 
            var claimsIdentity = new ClaimsIdentity(
            [
                new (JwtRegisteredClaimNames.Sub, user.Id ?? throw new InvalidOperationException()),
                new (JwtRegisteredClaimNames.Email, user.Email ?? throw new InvalidOperationException()),
                new (JwtRegisteredClaimNames.Name, user.UserName ?? throw new InvalidOperationException()),
            ]);
            // add additional claims createad above too
            claimsIdentity.AddClaims(userClaims);

            // 6. Createeate token
            var token = _ident.CreateSecurityToken(claimsIdentity);
            var tokenString = _ident.WriteToken(token);

            return new UserAuthResponseDto { User = user.ToDto(), Token = tokenString };
        }

        public async Task<string> Register(UserRegisterRequestDto request)
        {
            // 1. Create User
            var identity = new ApplicationUser
            {
                Email = request.UserName,
                UserName = request.UserName,
                Name = request.Name,
                PhoneNumber = request.PhoneNumber
            };

            // 2. Persist user to storage
            try
            {
                var result = await _um.CreateAsync(identity, request.Password);

                if (result.Succeeded) return string.Empty;

                throw new Exception($"User Registration failed with errors: {string.Join(",", result.Errors.Select(x => x.Description))}");
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}