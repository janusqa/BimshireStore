using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

// Implement custom data annotation validators by extending ValidationAttribute
namespace BimshireStore.Utility
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSize;

        public MaxFileSizeAttribute(long maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(object? value /*property being validated*/, ValidationContext validationContext)
        {
            long fileSize = 0;

            if (value is IBrowserFile browserFile)
            {
                fileSize = browserFile.Size;
            }
            else if (value is IFormFile formFile)
            {
                fileSize = formFile.Length;
            }

            if (fileSize > _maxFileSize * 1024 * 1024) // ensure files larger than 1MB are not uploadable
            {
                return new ValidationResult($"Maximum allowed file size is {_maxFileSize} MB");
            }

            return ValidationResult.Success;
        }

    }
}