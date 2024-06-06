using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BimshireStore.Models;
using BimshireStore.Services.IService;
using System.Text.Json;
using BimshireStore.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using BimshireStore.Utility;

namespace BimshireStore.Controllers;

[Authorize(Roles = $"{SD.Role_Admin}")]
public class ProductController : Controller
{
    private readonly ILogger<ProductController> _logger;
    private readonly IProductService _productService;


    public ProductController(ILogger<ProductController> logger, IProductService productService)
    {
        _logger = logger;
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> ProductIndex()
    {
        var response = await _productService.GetAllAsync();

        if (response is not null && response.IsSuccess)
        {
            var products = JsonSerializer.Deserialize<List<ProductDto>>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            return View(products);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return View(new List<ProductDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> ProductCreate()
    {
        await Task.CompletedTask;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ProductCreate(ProductDto product)
    {
        if (ModelState.IsValid)
        {
            var response = await _productService.CreateAsync(product);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            }
        }

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> ProductEdit(int productId)
    {
        var response = await _productService.GetByIdAsync(productId);

        if (response is not null && response.IsSuccess)
        {
            var product = JsonSerializer.Deserialize<ProductDto>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            return View(product);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProductEdit(ProductDto product)
    {
        if (ModelState.IsValid)
        {
            var response = await _productService.UpdateAsync(product);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
                return RedirectToAction(nameof(ProductIndex));
            }
            else
            {
                TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            }
        }

        return View(product);
    }

    [HttpGet]
    public async Task<IActionResult> ProductDelete(int productId)
    {
        var response = await _productService.GetByIdAsync(productId);

        if (response is not null && response.IsSuccess)
        {
            var product = JsonSerializer.Deserialize<ProductDto>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            return View(product);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> ProductDelete(ProductDto product)
    {

        var response = await _productService.DeleteAsync(product.ProductId);
        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Operation completed successfully";
            return RedirectToAction(nameof(ProductIndex));
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
        }

        return View(product);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
