using System.Globalization;
using BimshireStore.Services.AuthAPI.Data;
using BimshireStore.Services.AuthAPI.Models.Dto;
using BimshireStore.Services.AuthAPI.Services.IService;
using Microsoft.AspNetCore.Mvc;

namespace BimshireStore.Services.AuthAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]  // hard coded route name
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuthService _auth;
        private readonly IMessageBusSender _mbs;
        private readonly IConfiguration _config;

        public AuthController(ApplicationDbContext db, IAuthService auth, IMessageBusSender mbs, IConfiguration config)
        {
            _db = db;
            _auth = auth;
            _mbs = mbs;
            _config = config;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Register([FromBody] UserRegisterRequestDto request)
        {
            var result = await _auth.Register(request);

            if (!string.IsNullOrWhiteSpace(result))
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = [result],
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    }
                )
                { StatusCode = StatusCodes.Status400BadRequest };
            }

            _mbs.SendMessage(
                request.UserName,
                _config.GetValue<string>("MessageBus:TopicAndQueueNames:RegisterUserQueue")
                    ?? throw new InvalidOperationException("Invalid MessageBus Topic/Queue Name")
            );

            return Ok(
                new ApiResponse
                {
                    IsSuccess = true,
                    StatusCode = System.Net.HttpStatusCode.OK
                });
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] UserLoginRequestDto request)
        {
            var result = await _auth.Login(request);

            if (string.IsNullOrWhiteSpace(result.Token))
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        ErrorMessages = ["Unauthorized"],
                        StatusCode = System.Net.HttpStatusCode.Unauthorized
                    }
                )
                { StatusCode = StatusCodes.Status401Unauthorized };
            }

            return Ok(
                new ApiResponse
                {
                    IsSuccess = true,
                    Result = result,
                    StatusCode = System.Net.HttpStatusCode.OK
                });
        }

        [HttpPost("assign-role")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> AssignRole([FromBody] UserRegisterRequestDto request)
        {
            if (request.Role is not null)
            {
                TextInfo info = CultureInfo.CurrentCulture.TextInfo;
                var userAssignedToRole = await _auth.AssignRole(request.UserName, info.ToTitleCase(request.Role));
                if (userAssignedToRole)
                {
                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }
            }

            return new ObjectResult(
                new ApiResponse
                {
                    IsSuccess = false,
                    ErrorMessages = ["Cannot assign user to role"],
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                }
            )
            { StatusCode = StatusCodes.Status400BadRequest };
        }
    }
}