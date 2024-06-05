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
using BimshireStore.Utility;

namespace BimshireStore.Controllers;

public class OrderController : Controller
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderService _orderService;

    public OrderController(ILogger<OrderController> logger, IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> OrderIndex()
    {
        await Task.CompletedTask;
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _orderService.GetAllAsync();
        if (response is not null && response.IsSuccess)
        {
            var orders = JsonSerializer.Deserialize<List<OrderHeaderDto>>(JsonSerializer.Serialize(response.Result), JsonSerializerConfig.DefaultOptions);
            return Json(new { data = orders });
        }

        return Json(new { data = new List<OrderHeaderDto>() });
    }

    // [HttpGet]
    // public async Task<IActionResult> OrderDetail()
    // {
    //     await Task.CompletedTask;
    //     return View();
    // }

    // [HttpPost]
    // public async Task<IActionResult> OrderDetail(OrderDto order)
    // {
    //     if (ModelState.IsValid)
    //     {
    //         var response = await _couponService.CreateAsync(coupon);
    //         if (response is not null && response.IsSuccess)
    //         {
    //             TempData["success"] = "Operation completed successfully";
    //             return RedirectToAction(nameof(OrderIndex));
    //         }
    //         else
    //         {
    //             TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
    //         }
    //     }

    //     return View(coupon);
    // }



    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
