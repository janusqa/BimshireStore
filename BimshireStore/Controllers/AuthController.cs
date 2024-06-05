using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BimshireStore.Models;
using BimshireStore.Services.IService;
using System.Text.Json;
using BimshireStore.Models.Dto;
using static BimshireStore.Utility.SD;
using Microsoft.AspNetCore.Mvc.Rendering;
using BimshireStore.Utility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BimshireStore.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService, ITokenService tokenService)
    {
        _logger = logger;
        _authService = authService;
        _tokenService = tokenService;
    }


    [HttpGet]
    public async Task<IActionResult> Login()
    {
        await Task.CompletedTask;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(UserLoginRequestDto request)
    {
        var authenticated = await _authService.LoginAsync(request);

        if (authenticated is not null && authenticated.IsSuccess)
        {
            var result = JsonSerializer.Deserialize<UserAuthResponseDto>(JsonSerializer.Serialize(authenticated.Result), JsonSerializerConfig.DefaultOptions);
            if (result is not null)
            {
                // do not forget to set up cookie auth in program.cs
                await SignInUser(result.Token);
                _tokenService.SetToken(result.Token);
            }
            return RedirectToAction(nameof(Index), "Home");
        }
        else
        {
            var errorMessage = string.Join(" | ", authenticated?.ErrorMessages ?? ["Login failed"]);
            ModelState.AddModelError("CustomError", errorMessage);
            TempData["error"] = errorMessage;
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Register()
    {
        var roles = new List<SelectListItem> {
            new SelectListItem { Text = SD.Role_Admin, Value=Role_Admin},
            new SelectListItem { Text = SD.Role_Customer, Value=Role_Customer}
        };
        ViewBag.Roles = roles;

        await Task.CompletedTask;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(UserRegisterRequestDto request)
    {
        var roles = new List<SelectListItem> {
            new SelectListItem { Text = SD.Role_Admin, Value=Role_Admin},
            new SelectListItem { Text = SD.Role_Customer, Value=Role_Customer}
        };
        ViewBag.Roles = roles;

        var registerUser = await _authService.RegisterAsync(request);

        if (registerUser is not null && registerUser.IsSuccess)
        {
            if (string.IsNullOrWhiteSpace(request.Role)) request.Role = SD.Role_Customer;
            var assignUserToRole = await _authService.AssignRoleAsync(request);
            if (assignUserToRole is not null && registerUser.IsSuccess)
            {
                TempData["success"] = "Registration successful";
                return RedirectToAction(nameof(Login));
            }
        }

        TempData["error"] = string.Join(" | ", registerUser?.ErrorMessages ?? ["Registration failed"]);
        return View(request);

    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        _tokenService.ClearToken();
        return RedirectToAction(nameof(Login), "Auth");
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private async Task SignInUser(string token)
    {
        // Check the validity of the token and return success or a failure
        // Note MUST set "MapInboundClaims" to true when creating JsonWebTokenHandler
        // so that claims are populated properly and that authorization will work
        // properly in the controllers. I.E the User object will be populated properly
        // package: Microsoft.IdentityModel.JsonWebTokens
        var jwtTokenHandler = new JsonWebTokenHandler { MapInboundClaims = true };
        var jwtToken = jwtTokenHandler.ReadJsonWebToken(token);

        var claims = jwtToken.Claims.Select(x => new Claim(x.Type, x.Value)).ToList();

        // Need to specifically add this claimtype to populate User.Identity.Name on the front end
        claims.Add(new Claim(ClaimTypes.Name, jwtToken.Claims.First(x => x.Type == "name").Value));
        claims.Add(new Claim(ClaimTypes.Role, jwtToken.Claims.First(x => x.Type == "role").Value));

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);
    }
}
