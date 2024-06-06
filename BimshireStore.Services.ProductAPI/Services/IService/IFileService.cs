using Microsoft.AspNetCore.Components.Forms;

namespace BimshireStore.Services.ProductAPI.Services.IService
{
    public interface IFileService
    {
        Task<(string? FileUrl, string? Error)> CreateFile(IBrowserFile file, string? existingFileUrl = null);
        Task<(string? FileUrl, string? Error)> CreateFileSSR(IFormFile file, string? existingFileUrl = null);
        bool DeleteFile(string? existingFileUrl);
    }
}