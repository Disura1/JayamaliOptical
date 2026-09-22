using Microsoft.AspNetCore.Http;

namespace JayamaliOptical.Web.Services
{
    public static class FileUploadValidator
    {
        private static readonly string[] ImageExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] ImageContentTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };
        private const long ImageMaxSizeBytes = 5 * 1024 * 1024;

        private static readonly string[] PrescriptionExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
        private static readonly string[] PrescriptionContentTypes = { "image/jpeg", "image/png", "image/webp", "application/pdf" };
        private const long PrescriptionMaxSizeBytes = 10 * 1024 * 1024;

        public static bool IsValidImage(IFormFile file, out string? error) =>
            IsValid(file, ImageExtensions, ImageContentTypes, ImageMaxSizeBytes, out error);

        public static bool IsValidImage(IFormFile file, long maxSizeBytes, out string? error) =>
            IsValid(file, ImageExtensions, ImageContentTypes, maxSizeBytes, out error);

        public static bool IsValidPrescriptionFile(IFormFile file, out string? error) =>
            IsValid(file, PrescriptionExtensions, PrescriptionContentTypes, PrescriptionMaxSizeBytes, out error);

        private static bool IsValid(IFormFile file, string[] allowedExtensions, string[] allowedContentTypes, long maxSizeBytes, out string? error)
        {
            error = null;

            if (file.Length == 0)
            {
                error = "The selected file is empty.";
                return false;
            }

            if (file.Length > maxSizeBytes)
            {
                error = $"File is too large. Maximum allowed size is {maxSizeBytes / (1024 * 1024)} MB.";
                return false;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !allowedExtensions.Contains(extension))
            {
                error = $"Unsupported file type. Allowed types: {string.Join(", ", allowedExtensions)}.";
                return false;
            }

            if (string.IsNullOrEmpty(file.ContentType) || !allowedContentTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                error = "Unsupported file type.";
                return false;
            }

            return true;
        }
    }
}
