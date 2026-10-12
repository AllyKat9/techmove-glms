
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using TechMoveGLMS.API.Services;

namespace TechMoveGLMS.Tests.Tests.Unit
{
    public class FileValidationTests
    {
        private readonly FileUploadService _service;
        private readonly Mock<ILogger<FileUploadService>> _mockLogger;

        public FileValidationTests()
        {
            _mockLogger = new Mock<ILogger<FileUploadService>>();
            _service = new FileUploadService(_mockLogger.Object);
        }

        // Tests that a valid PDF file returns true
        [Fact]
        public void IsValidPdfFile_WithValidPdf_ReturnsTrue()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.pdf", "application/pdf", 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile);

            // Assert
            Assert.True(result);

        }

        // Tests that a valid PDF file returns true
        [Fact]
        public void IsValidPdfFile_WithExeFile_ReturnsFalse()
        {
            // Arrange
            var mockFile = CreateMockFormFile("virus.exe", "application/x-msdownload", 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile);

            // Assert
            Assert.False(result);
        }

        // Tests that text files are rejected
        [Fact]
        public void IsValidPdfFile_WithTxtFile_ReturnsFalse()
        {
            // Arrange
            var mockFile = CreateMockFormFile("document.txt", "text/plain", 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile);

            // Assert
            Assert.False(result);
        }

        // Tests that empty files are rejected
        [Fact]
        public void IsValidPdfFile_WithEmptyFile_ReturnsFalse()
        {
            // Arrange
            var mockFile = CreateMockFormFile("empty.pdf", "application/pdf", 0);

            // Act
            var result = _service.IsValidPdfFile(mockFile);

            // Assert
            Assert.False(result);
        }

        // Tests that null files are rejected
        [Fact]
        public void IsValidPdfFile_WithNullFile_ReturnsFalse()
        {
            // Act
            var result = _service.IsValidPdfFile(null);

            // Assert
            Assert.False(result);
        }

        // Tests that files of a wrong content type are rejected
        [Fact]
        public void IsValidPdfFile_WithWrongContentType_ReturnsFalse()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.pdf", "image/jpeg", 1024);

            // Act
            var result = _service.IsValidPdfFile(mockFile);

            // Assert
            Assert.False(result);
        }

        // Tests that empty files are rejected
        [Fact]
        public async Task UploadPdfAsync_WithValidFile_SavesToDisk()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.pdf", "application/pdf", 1024);
            var tempPath = Path.GetTempPath();
            var uploadFolder = Path.Combine(tempPath, "test_uploads");

            try
            {
                // Act
                var result = await _service.UploadPdfAsync(mockFile, uploadFolder);

                // Assert
                Assert.True(result.success);
                Assert.NotEmpty(result.filePath);
                Assert.True(File.Exists(result.filePath));
            }
            finally
            {
                
                if (Directory.Exists(uploadFolder))
                {
                    Directory.Delete(uploadFolder, true);
                }
            }
        }

        // Tests that invalid files aren't uploaded
        [Fact]
        public async Task UploadPdfAsync_WithInvalidFile_ReturnsError()
        {
            // Arrange
            var mockFile = CreateMockFormFile("test.exe", "application/x-msdownload", 1024);
            var tempPath = Path.GetTempPath();

            // Act
            var result = await _service.UploadPdfAsync(mockFile, tempPath);

            // Assert
            Assert.False(result.success);
            Assert.Contains("Only PDF files are allowed", result.errorMessage);
        }

        // Tests that large files are rejected
        [Fact]
        public async Task UploadPdfAsync_WithFileTooLarge_ReturnsError()
        {
            // Arrange
            var mockFile = CreateMockFormFile("large.pdf", "application/pdf", 11 * 1024 * 1024);
            var tempPath = Path.GetTempPath();

            // Act
            var result = await _service.UploadPdfAsync(mockFile, tempPath);

            // Assert
            Assert.False(result.success);
            Assert.Contains("max 10MB", result.errorMessage);
        }

        private IFormFile CreateMockFormFile(string fileName, string contentType, long length)
        {
            var stream = new MemoryStream(new byte[length]);
            var mockFile = new Mock<IFormFile>();

            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.ContentType).Returns(contentType);
            mockFile.Setup(f => f.Length).Returns(length);
            mockFile.Setup(f => f.OpenReadStream()).Returns(stream);

            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));

            return mockFile.Object;
        }
    }
}
