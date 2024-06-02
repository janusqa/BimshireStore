using System.Security.Claims;
using System.Text.Json;
using BimshireStore.Models.Dto;
using BimshireStore.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using static BimshireStore.Utility.SD;

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

            var orderCreated = await _orderService.CreateOrder(cart);

            if (orderCreated is not null && orderCreated.IsSuccess)
            {
                var orderHeader = JsonSerializer.Deserialize<OrderHeaderDto>(JsonSerializer.Serialize(orderCreated.Result), JsonSerializerConfig.DefaultOptions);
                Console.WriteLine(orderHeader);
            }

            return View(cart);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            await Task.CompletedTask;
            return View(orderId);
        }

        private async Task<CartDto> GetCartByUserIdAsync()
        {
            var userId = (User.Identity as ClaimsIdentity)?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (userId is not null)
            {
                var response = await _cartService.GetByUserIdAsync(userId);

                if (response is not null && response.IsSuccess)
                {
                    var cart = JsonSerializer.Deserialize<CartDto>(JsonSerializer.Serialize(response.Result), JsonSerializerConfig.DefaultOptions);
                    if (cart is not null) return cart;
                }
            }

            return new CartDto { CartHeader = new CartHeaderDto() };
        }
    }
}