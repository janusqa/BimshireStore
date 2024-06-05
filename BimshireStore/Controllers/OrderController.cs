using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BimshireStore.Models;
using BimshireStore.Services.IService;
using System.Text.Json;
using BimshireStore.Models.Dto;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<IActionResult> OrderDetail(int orderId)
    {
        var response = await _orderService.GetByIdAsync(orderId);

        if (response is not null && response.IsSuccess)
        {
            var order = JsonSerializer.Deserialize<OrderHeaderDto>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            if (order is not null)
            {
                return View(order);
            }
        }

        return NotFound();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> ReadyForPickup(int orderId)
    {
        var response = await _orderService.SetStatusAsync(orderId, SD.Status_ReadyForPickup);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Operation completed successfully";
            return RedirectToAction(nameof(OrderDetail), "Order", new { orderId = orderId });
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CompleteOrder(int orderId)
    {
        var response = await _orderService.SetStatusAsync(orderId, SD.Status_Completed);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Operation completed successfully";
            return RedirectToAction(nameof(OrderDetail), "Order", new { orderId = orderId });
        }

        return View();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CancelOrder(int orderId)
    {
        var response = await _orderService.SetStatusAsync(orderId, SD.Status_Cancelled);

        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Operation completed successfully";
            return RedirectToAction(nameof(OrderDetail), "Order", new { orderId = orderId });
        }

        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    #region REST API 
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll(string status)
    {
        var response = await _orderService.GetAllAsync();
        if (response is not null && response.IsSuccess)
        {
            var orders = JsonSerializer.Deserialize<List<OrderHeaderDto>>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            var statusFilter = status switch
            {
                "approved" => SD.Status_Approved,
                "readyforpickup" => SD.Status_ReadyForPickup,
                "cancelled" => SD.Status_Cancelled,
                _ => "All"
            };
            return Json(new { data = (statusFilter == "All" ? orders : orders?.Where(x => x.Status == statusFilter)) ?? [] });
        }

        return Json(new { data = new List<OrderHeaderDto>() });
    }
    #endregion
}
