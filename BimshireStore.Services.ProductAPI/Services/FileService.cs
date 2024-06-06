using BimshireStore.Services.ProductAPI.Services.IService;
using Microsoft.AspNetCore.Components.Forms;

namespace BimshireStore.Services.ProductAPI.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _whe;

        public FileService(IWebHostEnvironment whe)
        {
            _whe = whe;
        }

        public bool DeleteFile(string? existingFileUrl)
        {
            if (!string.IsNullOrWhiteSpace(existingFileUrl))
            {
                string wwwRootPath = _whe.WebRootPath;
                string urlPath = $@"";
                string fileDirectory = Path.Combine(wwwRootPath, urlPath);
                var existingFile = Path.Combine(fileDirectory, existingFileUrl[1..]);

                if (System.IO.File.Exists(existingFile))
                {
                    System.IO.File.Delete(existingFile);
                    return true;
                }
            }
            return false;
        }

        public async Task<(string? FileUrl, string? Error)> CreateFile(IBrowserFile file, string? existingFileUrl = null)
        {
            string? FileUrl = null;

            try
            {
                if (file is not null)
                {
                    string wwwRootPath = _whe.WebRootPath;
                    string urlPath = $@"ProductImages";
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
                    string fileDirectory = Path.Combine(wwwRootPath, urlPath);
                    string filePath = Path.Combine(fileDirectory, fileName);

                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    // if a file was uploaded and there is an existing file
                    // we need to repalce the existing file by first deleting 
                    // it and then copying in the new file. Otherwise, just
                    // copy in the new file
                    DeleteFile(existingFileUrl);


                    using (FileStream writer = new(filePath, FileMode.Create))
                    {
                        await file.OpenReadStream().CopyToAsync(writer);
                    }

                    FileUrl = @$"/{urlPath}/{fileName}";
                }
                else
                {
                    // if no file is uploaded but a file is already in the database
                    // keep that file, otherwise set ImageUrl to empty string.
                    FileUrl =
                        string.IsNullOrWhiteSpace(existingFileUrl) ? "" : existingFileUrl;
                }

                return (FileUrl, null);
            }
            catch (Exception ex)
            {
                return (FileUrl, ex.Message);
            }
        }

        public async Task<(string? FileUrl, string? Error)> CreateFileSSR(IFormFile file, string? existingFileUrl = null)
        {
            string? FileUrl = null;

            try
            {
                if (file is not null)
                {
                    string wwwRootPath = _whe.WebRootPath;
                    string urlPath = $@"ProductImages";
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    string fileDirectory = Path.Combine(wwwRootPath, urlPath);
                    string filePath = Path.Combine(fileDirectory, fileName);

                    if (!Directory.Exists(fileDirectory))
                    {
                        Directory.CreateDirectory(fileDirectory);
                    }

                    // if a file was uploaded and there is an existing file
                    // we need to repalce the existing file by first deleting 
                    // it and then copying in the new file. Otherwise, just
                    // copy in the new file
                    DeleteFile(existingFileUrl);

                    using (FileStream writer = new(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(writer);
                    }

                    FileUrl = @$"/{urlPath}/{fileName}";
                }
                else
                {
                    // if no file is uploaded but a file is already in the database
                    // keep that file, otherwise set ImageUrl to empty string.
                    FileUrl =
                        string.IsNullOrWhiteSpace(existingFileUrl) ? "" : existingFileUrl;
                }
                return (FileUrl, null);
            }
            catch (Exception ex)
            {
                return (FileUrl, ex.Message);
            }
        }
    }
}