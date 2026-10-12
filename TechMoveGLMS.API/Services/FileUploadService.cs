namespace TechMoveGLMS.API.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(ILogger<FileUploadService> logger)
        {
            _logger = logger;
        }

        public bool IsValidPdfFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                return false;

            if (file.ContentType != "application/pdf")
                return false;

            if (file.Length > 10 * 1024 * 1024)
                return false;

            return true;
        }

        public async Task<(bool success, string filePath, string errorMessage)> UploadPdfAsync(IFormFile file, string uploadFolder)
        {
            try
            {
                if (!IsValidPdfFile(file))
                {
                    return (false, string.Empty, "Invalid file. Only PDF files are allowed (max 10MB)");
                }

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, filePath, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return (false, string.Empty, $"Upload failed: {ex.Message}");
            }
        }
    }
}

