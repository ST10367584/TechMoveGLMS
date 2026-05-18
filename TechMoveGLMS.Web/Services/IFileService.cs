namespace TechMoveGLMS.Web.Services;

public interface IFileService
{
    Task<(string savedPath, string originalName)> SaveContractPdfAsync(IFormFile file, int contractId);
    void ValidateFile(IFormFile file);
    bool DeleteFile(string filePath);
}