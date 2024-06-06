using BimshireStore.Services.ProductAPI.Data;
using BimshireStore.Services.ProductAPI.Models;
using BimshireStore.Services.ProductAPI.Models.Dto;
using BimshireStore.Services.ProductAPI.Models.Extensions;
using BimshireStore.Services.ProductAPI.Services.IService;
using BimshireStore.Services.ProductAPI.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BimshireStore.Services.ProductAPI.Controllers
{
    [ApiController]
    [Route("api/products")]  // hard coded route name
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileService _fileService;

        public ProductController(ApplicationDbContext db, IFileService fileService)
        {
            _db = db;
            _fileService = fileService;
        }

        [HttpGet("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetAll()
        {
            try
            {
                var products = (await _db.Products.ToListAsync()).Select(x => x.ToDto());

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = products,
                        StatusCode = System.Net.HttpStatusCode.OK
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [HttpGet("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetById([FromRoute] int Id)
        {
            try
            {
                var product = (await _db.Products.FirstOrDefaultAsync(x => x.ProductId == Id))?.ToDto();

                if (product is not null)
                {
                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            Result = product,
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }
                else
                {
                    return NotFound(new ApiResponse { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.NotFound });
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize(Roles = $"{SD.Role_Admin}")]
        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Post([FromForm] ProductDto productDto)
        {
            try
            {
                var product = new Product
                {
                    Name = productDto.Name,
                    Price = productDto.Price,
                    Description = productDto.Description,
                    CategoryName = productDto.CategoryName,
                    ImageUrl = "https://placehold.co/600x400"
                };

                if (productDto.File is not null)
                {
                    var (FileUrl, Error) = await _fileService.CreateFileSSR(productDto.File);

                    if (Error is null && !string.IsNullOrWhiteSpace(FileUrl))
                    {
                        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                        product.ImageLocalPath = FileUrl;
                        product.ImageUrl = $"{baseUrl}{FileUrl}";
                    }
                }

                await _db.Products.AddAsync(product);
                await _db.SaveChangesAsync();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = product.ToDto(),
                        StatusCode = System.Net.HttpStatusCode.OK
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize(Roles = $"{SD.Role_Admin}")]
        [HttpPut("")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Put([FromForm] ProductDto productDto)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == productDto.ProductId);

                if (product is null)
                {
                    return NotFound(
                        new ApiResponse
                        {
                            IsSuccess = false,
                            StatusCode = System.Net.HttpStatusCode.NotFound
                        });
                };

                product.Name = productDto.Name;
                product.Price = productDto.Price;
                product.Description = productDto.Description;
                product.CategoryName = productDto.CategoryName;

                if (productDto.File is not null)
                {
                    var (FileUrl, Error) = await _fileService.CreateFileSSR(productDto.File, product.ImageLocalPath);

                    if (Error is null && !string.IsNullOrWhiteSpace(FileUrl))
                    {
                        var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
                        product.ImageLocalPath = FileUrl;
                        product.ImageUrl = $"{baseUrl}{FileUrl}";
                    }
                }

                _db.Products.Update(product);
                await _db.SaveChangesAsync();

                return Ok(
                    new ApiResponse
                    {
                        IsSuccess = true,
                        Result = product.ToDto(),
                        StatusCode = System.Net.HttpStatusCode.OK
                    });
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

        [Authorize(Roles = $"{SD.Role_Admin}")]
        [HttpDelete("{Id:int}")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> Delete([FromRoute] int Id)
        {
            try
            {
                var product = await _db.Products.FirstOrDefaultAsync(x => x.ProductId == Id);
                if (product is not null)
                {
                    var existingFileUrl = product.ImageLocalPath;
                    _db.Products.Remove(product);
                    await _db.SaveChangesAsync();
                    _fileService.DeleteFile(existingFileUrl);


                    return Ok(
                        new ApiResponse
                        {
                            IsSuccess = true,
                            Result = product.ToDto(),
                            StatusCode = System.Net.HttpStatusCode.OK
                        });
                }
                else
                {
                    return NotFound(new ApiResponse { IsSuccess = false, StatusCode = System.Net.HttpStatusCode.NotFound });
                }
            }
            catch (Exception ex)
            {
                return new ObjectResult(
                    new ApiResponse
                    {
                        IsSuccess = false,
                        Result = null,
                        ErrorMessages = [ex.Message],
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    }
                )
                { StatusCode = StatusCodes.Status500InternalServerError };
            }
        }

    }
}