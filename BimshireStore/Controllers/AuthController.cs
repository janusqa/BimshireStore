using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BimshireStore.Models;
using BimshireStore.Services.IService;
using System.Text.Json;
using BimshireStore.Models.Dto;
using static BimshireStore.Utility.SD;
using Microsoft.AspNetCore.Mvc.Rendering;
using BimshireStore.Utility;

namespace BimshireStore.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;

    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
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
        var response = await _authService.LoginAsync(request);

        if (response is not null && response.IsSuccess)
        {
            var result = JsonSerializer.Deserialize<UserAuthResponseDto>(JsonSerializer.Serialize(response.Result), JsonSerializerConfig.DefaultOptions);
            return View(result);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
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
        var response = await _authService.RegisterAsync(request);

        if (response is not null && response.IsSuccess)
        {
            return View();
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await Task.CompletedTask;
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
