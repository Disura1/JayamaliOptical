using JayamaliOptical.Web.Services;
using Microsoft.AspNetCore.Http;

namespace JayamaliOptical.Tests.Services
{
    public class FileUploadValidatorTests
    {
        private static IFormFile MakeFile(string fileName, string contentType, long length)
        {
            var stream = new MemoryStream(new byte[Math.Max(length, 1)]);
            return new FormFile(stream, 0, length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };
        }

        [Theory]
        [InlineData("photo.jpg", "image/jpeg")]
        [InlineData("photo.png", "image/png")]
        [InlineData("photo.webp", "image/webp")]
        public void IsValidImage_AllowedTypes_ReturnsTrue(string fileName, string contentType)
        {
            var file = MakeFile(fileName, contentType, 1024);

            var result = FileUploadValidator.IsValidImage(file, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValidImage_EmptyFile_ReturnsFalse()
        {
            var file = MakeFile("photo.jpg", "image/jpeg", 0);

            var result = FileUploadValidator.IsValidImage(file, out var error);

            Assert.False(result);
            Assert.NotNull(error);
        }

        [Fact]
        public void IsValidImage_ExceedsMaxSize_ReturnsFalse()
        {
            var file = MakeFile("photo.jpg", "image/jpeg", 6 * 1024 * 1024);

            var result = FileUploadValidator.IsValidImage(file, out var error);

            Assert.False(result);
            Assert.Contains("too large", error);
        }

        [Fact]
        public void IsValidImage_DisallowedExtension_ReturnsFalse()
        {
            var file = MakeFile("malicious.exe", "image/jpeg", 1024);

            var result = FileUploadValidator.IsValidImage(file, out var error);

            Assert.False(result);
        }

        [Fact]
        public void IsValidImage_ExtensionContentTypeMismatch_ReturnsFalse()
        {
            // .jpg extension but an HTML payload masquerading with the content type header
            var file = MakeFile("photo.jpg", "text/html", 1024);

            var result = FileUploadValidator.IsValidImage(file, out var error);

            Assert.False(result);
        }

        [Theory]
        [InlineData("scan.pdf", "application/pdf")]
        [InlineData("scan.jpg", "image/jpeg")]
        public void IsValidPrescriptionFile_AllowedTypes_ReturnsTrue(string fileName, string contentType)
        {
            var file = MakeFile(fileName, contentType, 2048);

            var result = FileUploadValidator.IsValidPrescriptionFile(file, out var error);

            Assert.True(result);
            Assert.Null(error);
        }

        [Fact]
        public void IsValidPrescriptionFile_DisallowedExtension_ReturnsFalse()
        {
            var file = MakeFile("scan.exe", "application/octet-stream", 2048);

            var result = FileUploadValidator.IsValidPrescriptionFile(file, out var error);

            Assert.False(result);
        }
    }
}
