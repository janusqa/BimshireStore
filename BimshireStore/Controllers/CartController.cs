using System.Security.Claims;
using System.Text.Json;
using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using BimshireStore.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace BimshireStore.Controllers
{

    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ILogger<CartController> logger, ICartService cartService, IOrderService orderService)
        {
            _logger = logger;
            _cartService = cartService;
            _orderService = orderService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> CartIndex()
        {
            var cart = await GetCartByUserIdAsync();
            return View(cart);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> RemoveItem(int cartDetailId)
        {
            var response = await _cartService.RemoveItemAsync(cartDetailId);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
                return RedirectToAction(nameof(CartIndex), "Cart");
            }

            TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            return RedirectToAction(nameof(CartIndex), "Cart");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(CartDto cart)
        {

            var response = await _cartService.ApplyCouponAsync(cart);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
            }
            else
            {
                TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            }

            return RedirectToAction(nameof(CartIndex), "Cart");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveCoupon(CartDto cart)
        {
            cart.CartHeader.CouponCode = null;

            var response = await _cartService.ApplyCouponAsync(cart);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
            }
            else
            {
                TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            }

            return RedirectToAction(nameof(CartIndex), "Cart");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> EmailCart(CartDto cart)
        {
            var userEmail = (User.Identity as ClaimsIdentity)?.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
            var cartFromDb = await GetCartByUserIdAsync();
            cartFromDb.CartHeader.Email = userEmail;

            var response = await _cartService.EmailCartAsync(cartFromDb);
            if (response is not null && response.IsSuccess)
            {
                TempData["success"] = "Operation completed successfully";
            }
            else
            {
                TempData["error"] = string.Join(" | ", response?.ErrorMessages ?? ["Oops, Something went wrong"]);
            }

            return RedirectToAction(nameof(CartIndex), "Cart");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = await GetCartByUserIdAsync();
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CartDto cartDto)
        {
            var cart = await GetCartByUserIdAsync();

            cart.CartHeader.Email = cartDto.CartHeader.Email;
            cart.CartHeader.Name = cartDto.CartHeader.Name;
            cart.CartHeader.Phone = cartDto.CartHeader.Phone;

            var orderCreated = await _orderService.CreateOrderAsync(cart);

            if (orderCreated is not null && orderCreated.IsSuccess)
            {
                var orderHeader = JsonSerializer.Deserialize<OrderHeaderDto>(JsonSerializer.Serialize(orderCreated.Result), SD.JsonSerializerConfig.DefaultOptions);
                if (orderHeader is not null)
                {
                    var stripeRequest = new StripeRequest
                    {
                        ApprovedUrl = $"{SD.AppBaseAddress}/Cart/Confirmation?orderId={orderHeader.OrderHeaderId}",
                        CancelledUrl = $"{SD.AppBaseAddress}/Cart/Checkout",
                        OrderHeaderDto = orderHeader,
                        StripeSessionId = string.Empty,
                        StripeSessionUrl = string.Empty
                    };

                    var response = await _orderService.CreateStripeSessionAsync(stripeRequest);
                    if (response is not null && response.IsSuccess)
                    {
                        stripeRequest = JsonSerializer.Deserialize<StripeRequest>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
                        if (stripeRequest is not null) Response.Headers.Append("Location", stripeRequest.StripeSessionUrl);
                        return new StatusCodeResult(303);
                    }
                }
            }

            TempData["error"] = "There was an error processing your order. Please try again later";
            return View(cart);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var response = await _orderService.ValidateStripeSessionAsync(orderId);
            if (response is not null && response.IsSuccess)
            {
                var orderHeader = JsonSerializer.Deserialize<OrderHeaderDto>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
                if (orderHeader?.Status == SD.Status_Approved)
                {
                    return View(orderId);
                }
            }

            var reason = response?.ErrorMessages?.Count > 0 ? string.Join(" | ", response.ErrorMessages) : "Unknown";
            TempData["error"] = $"Order failed for reason: {reason}";

            return RedirectToAction(nameof(OrderFail), "Cart");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> OrderFail()
        {
            await Task.CompletedTask;
            return View();
        }

        private async Task<CartDto> GetCartByUserIdAsync()
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId is not null)
            {
                var response = await _cartService.GetByUserIdAsync(userId);

                if (response is not null && response.IsSuccess)
                {
                    var cart = JsonSerializer.Deserialize<CartDto>(JsonSerializer.Serialize(response.Result), SD.JsonSerializerConfig.DefaultOptions);
                    if (cart is not null) return cart;
                }
            }

            return new CartDto { CartHeader = new CartHeaderDto() };
        }
    }
}