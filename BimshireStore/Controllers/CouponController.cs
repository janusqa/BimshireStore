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
public class CouponController : Controller
{
    private readonly ILogger<CouponController> _logger;
    private readonly ICouponService _couponService;

    public CouponController(ILogger<CouponController> logger, ICouponService couponService)
    {
        _logger = logger;
        _couponService = couponService;
    }

    [HttpGet]
    public async Task<IActionResult> CouponIndex()
    {
        var response = await _couponService.GetAllAsync();

        if (response is not null && response.IsSuccess)
        {
            var coupons = JsonSerializer.Deserialize<List<CouponDto>>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            return View(coupons);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return View(new List<CouponDto>());
        }
    }

    [HttpGet]
    public async Task<IActionResult> CouponCreate()
    {
        await Task.CompletedTask;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CouponCreate(CouponDto coupon)
    {
        if (ModelState.IsValid)
        {
            var response = await _couponService.CreateAsync(coupon);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
                return RedirectToAction(nameof(CouponIndex));
            }
            else
            {
                TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            }
        }

        return View(coupon);
    }

    [HttpGet]
    public async Task<IActionResult> CouponDelete(int couponId)
    {
        var response = await _couponService.GetByIdAsync(couponId);

        if (response is not null && response.IsSuccess)
        {
            var coupon = JsonSerializer.Deserialize<CouponDto>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
            return View(coupon);
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<IActionResult> CouponDelete(CouponDto coupon)
    {

        var response = await _couponService.DeleteAsync(coupon.CouponId);
        if (response is not null && response.IsSuccess)
        {
            TempData["success"] = "Operation completed successfully";
            return RedirectToAction(nameof(CouponIndex));
        }
        else
        {
            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
        }

        return View(coupon);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
