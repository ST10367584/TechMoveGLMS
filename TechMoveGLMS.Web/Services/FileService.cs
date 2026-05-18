namespace TechMoveGLMS.Web.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FileService> _logger;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase) { ".pdf" };
    private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase) { "application/pdf" };
    private const long MaxSize = 10 * 1024 * 1024;

    public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public void ValidateFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("No file uploaded.");
        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File type '{ext}' not allowed. Only PDF.");
        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new InvalidOperationException("Invalid MIME type. Only PDF.");
        if (file.Length > MaxSize)
            throw new InvalidOperationException("File exceeds 10 MB limit.");
    }

    public async Task<(string savedPath, string originalName)> SaveContractPdfAsync(IFormFile file, int contractId)
    {
        ValidateFile(file);
        var folder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
        Directory.CreateDirectory(folder);
        var originalName = Path.GetFileName(file.FileName);
        var uniqueName = $"contract_{contractId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
        var fullPath = Path.Combine(folder, uniqueName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
        _logger.LogInformation("Saved PDF for contract {ContractId}", contractId);
        return ($"/uploads/contracts/{uniqueName}", originalName);
    }

    public bool DeleteFile(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath)) File.Delete(fullPath);
            return true;
        }
        catch { return false; }
    }
}