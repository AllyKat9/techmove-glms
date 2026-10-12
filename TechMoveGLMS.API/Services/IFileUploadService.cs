namespace TechMoveGLMS.API.Services
{
    public interface IFileUploadService
    {
        Task<(bool success, string filePath, string errorMessage)> UploadPdfAsync(IFormFile file, string uploadFolder);
        bool IsValidPdfFile(IFormFile file);

    }
}
