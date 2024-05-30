using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BimshireStore.Models;
using BimshireStore.Services.IService;
using System.Text.Json;
using BimshireStore.Models.Dto;
using static BimshireStore.Utility.SD;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BimshireStore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly ICartService _cartService;

    public HomeController(ILogger<HomeController> logger, IProductService productService, ICartService cartService)
    {
        _logger = logger;
        _productService = productService;
        _cartService = cartService;
    }

    public async Task<IActionResult> Index()
    {
        var response = await _productService.GetAllAsync();

        if (response is not null && response.IsSuccess)
        {
            var products = JsonSerializer.Deserialize<List<ProductDto>>(JsonSerializer.Serialize(response.Result), JsonSerializerConfig.DefaultOptions);
            return View(products);
        }
        else
        {
            // TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return View(new List<ProductDto>());
        }
    }

    [Authorize]
    public async Task<IActionResult> ProductDetails(int productId)
    {
        var response = await _productService.GetByIdAsync(productId);

        if (response is not null && response.IsSuccess)
        {
            var product = JsonSerializer.Deserialize<ProductDto>(JsonSerializer.Serialize(response.Result), JsonSerializerConfig.DefaultOptions);
            return View(product);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return RedirectToAction(nameof(Index), "Home");
        }
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ProductDetails(ProductDto product)
    {
        var userId = (User.Identity as ClaimsIdentity)?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        var cart = new CartDto
        {
            CartHeader = new CartHeaderDto { UserId = userId },
            CartDetail = [new CartDetailDto { ProductId = product.ProductId, Count = product.Count }]
        };

        var response = await _cartService.UpsertItemAsync(cart);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Operation completed successfully";
            return RedirectToAction(nameof(Index), "Home");
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return View(product);
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
