namespace TechMoveGLMS.Web.Services
{
    public interface IFileService
    {
        Task<(string savedPath, string originalName)> SaveContractPdfAsync(IFormFile file, int contractId);
        void ValidateFile(IFormFile file);
        bool DeleteFile(string filePath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        // Only PDF is accepted — requirement from the assignment
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".pdf" };

        private static readonly HashSet<string> AllowedMimeTypes =
            new(StringComparer.OrdinalIgnoreCase) { "application/pdf" };

        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Validates that the file is a PDF and within size limits.
        /// Throws InvalidOperationException for disallowed types.
        /// This is pure validation logic — tested by unit tests.
        /// </summary>
        public void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file was uploaded.");

            var extension = Path.GetExtension(file.FileName);

            if (!AllowedExtensions.Contains(extension))
                throw new InvalidOperationException(
                    $"File type '{extension}' is not allowed. Only PDF files are accepted.");

            if (!AllowedMimeTypes.Contains(file.ContentType))
                throw new InvalidOperationException(
                    $"MIME type '{file.ContentType}' is not allowed. Only PDF files are accepted.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException(
                    $"File size exceeds the maximum allowed size of 10 MB.");
        }

        /// <summary>
        /// Saves the uploaded PDF to the server file system (simulated file server).
        /// Returns the relative saved path and the original filename.
        /// </summary>
        public async Task<(string savedPath, string originalName)> SaveContractPdfAsync(
            IFormFile file, int contractId)
        {
            ValidateFile(file);

            // Simulate a "file server" folder: wwwroot/uploads/contracts/
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "contracts");
            Directory.CreateDirectory(uploadsFolder);

            var originalName = Path.GetFileName(file.FileName);
            // Unique name: contract_{id}_{timestamp}.pdf
            var uniqueName = $"contract_{contractId}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            var fullPath = Path.Combine(uploadsFolder, uniqueName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            _logger.LogInformation("Saved PDF for contract {ContractId}: {Path}", contractId, fullPath);

            // Store the relative URL path for serving via browser
            var relativePath = $"/uploads/contracts/{uniqueName}";
            return (relativePath, originalName);
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file {Path}", filePath);
                return false;
            }
        }
    }
}
