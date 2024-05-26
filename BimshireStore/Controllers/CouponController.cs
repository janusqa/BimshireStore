using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BimshireStore.Models;
using BimshireStore.Service.IService;
using System.Text.Json;
using BimshireStore.Models.Dto;

namespace BimshireStore.Controllers;

public class CouponController : Controller
{
    private readonly ILogger<CouponController> _logger;
    private readonly ICouponService _couponService;

    public CouponController(ILogger<CouponController> logger, ICouponService couponService)
    {
        _logger = logger;
        _couponService = couponService;
    }

    public async Task<IActionResult> CouponIndex()
    {
        var response = await _couponService.GetAllCouponsAsync();
        if (response is not null && response.IsSuccess)
        {
            var coupons = JsonSerializer.Deserialize<List<CouponDto>>(JsonSerializer.Serialize(response.Result));
            return View(coupons);
        }
        else
        {
            return View(new List<CouponDto>());
        }

    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
