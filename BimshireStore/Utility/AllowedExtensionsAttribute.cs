using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

// Implement custom data annotation validators by extending ValidationAttribute
namespace BimshireStore.Utility
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? extension = null;

            if (value is IBrowserFile browserFile)
            {
                extension = Path.GetExtension(browserFile.Name);
            }
            else if (value is IFormFile formFile)
            {
                extension = Path.GetExtension(formFile.FileName);
            }

            if (!_extensions.Contains(extension?.ToLower()))
            {
                return new ValidationResult("This file extension is not allowed");
            }

            return ValidationResult.Success;
        }

    }
}