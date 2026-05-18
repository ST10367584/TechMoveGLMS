using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    /// <summary>
    /// Unit tests for FileService.ValidateFile()
    /// Verifies that only .pdf files are accepted and
    /// that restricted types (e.g., .exe, .docx, .jpg) are rejected.
    /// </summary>
    public class FileValidationTests
    {
        // ── Helper: build FileService without IWebHostEnvironment (not needed for ValidateFile) ───
        private static FileService BuildService()
        {
            var envMock = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            var logger = NullLogger<FileService>.Instance;
            return new FileService(envMock.Object, logger);
        }

        // ── Helper: create a mock IFormFile ───────────────────────────────────
        private static IFormFile MakeFakeFile(string fileName, string contentType, long sizeBytes = 1024)
        {
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            fileMock.Setup(f => f.Length).Returns(sizeBytes);
            return fileMock.Object;
        }

        // ── 1. Valid PDF passes without exception ─────────────────────────────
        [Fact]
        public void ValidateFile_ValidPdf_DoesNotThrow()
        {
            var service = BuildService();
            var file = MakeFakeFile("agreement.pdf", "application/pdf");

            // Act & Assert — no exception expected
            var ex = Record.Exception(() => service.ValidateFile(file));
            Assert.Null(ex);
        }

        // ── 2. .exe file is rejected ──────────────────────────────────────────
        [Fact]
        public void ValidateFile_ExeFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            var file = MakeFakeFile("virus.exe", "application/octet-stream");

            var ex = Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
            Assert.Contains(".exe", ex.Message);
        }

        // ── 3. .docx file is rejected ─────────────────────────────────────────
        [Fact]
        public void ValidateFile_DocxFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            var file = MakeFakeFile("contract.docx",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document");

            var ex = Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
            Assert.Contains(".docx", ex.Message);
        }

        // ── 4. .jpg image is rejected ─────────────────────────────────────────
        [Fact]
        public void ValidateFile_JpgFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            var file = MakeFakeFile("photo.jpg", "image/jpeg");

            Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
        }

        // ── 5. .png image is rejected ─────────────────────────────────────────
        [Fact]
        public void ValidateFile_PngFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            var file = MakeFakeFile("logo.png", "image/png");

            Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
        }

        // ── 6. .xlsx spreadsheet is rejected ──────────────────────────────────
        [Fact]
        public void ValidateFile_XlsxFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            var file = MakeFakeFile("data.xlsx",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
        }

        // ── 7. Empty file is rejected ─────────────────────────────────────────
        [Fact]
        public void ValidateFile_EmptyFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            var file = MakeFakeFile("empty.pdf", "application/pdf", sizeBytes: 0);

            var ex = Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
            Assert.Contains("No file", ex.Message);
        }

        // ── 8. File exceeding 10 MB is rejected ───────────────────────────────
        [Fact]
        public void ValidateFile_FileTooLarge_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            long elevenMb = 11 * 1024 * 1024;
            var file = MakeFakeFile("huge.pdf", "application/pdf", sizeBytes: elevenMb);

            var ex = Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
            Assert.Contains("10 MB", ex.Message);
        }

        // ── 9. PDF with wrong MIME type is rejected ────────────────────────────
        [Fact]
        public void ValidateFile_PdfExtensionButWrongMime_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            // A renamed .exe pretending to be PDF — correct extension, wrong MIME
            var file = MakeFakeFile("agreement.pdf", "application/octet-stream");

            Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
        }

        // ── 10. Null file reference is rejected ───────────────────────────────
        [Fact]
        public void ValidateFile_NullFile_ThrowsInvalidOperationException()
        {
            var service = BuildService();
            Assert.Throws<InvalidOperationException>(() => service.ValidateFile(null!));
        }

        // ── 11. Parametrized — multiple disallowed extensions ─────────────────
        [Theory]
        [InlineData("script.js", "application/javascript")]
        [InlineData("archive.zip", "application/zip")]
        [InlineData("page.html", "text/html")]
        [InlineData("data.csv", "text/csv")]
        [InlineData("app.bat", "application/x-bat")]
        public void ValidateFile_DisallowedExtensions_ThrowsInvalidOperationException(
            string fileName, string mime)
        {
            var service = BuildService();
            var file = MakeFakeFile(fileName, mime);
            Assert.Throws<InvalidOperationException>(() => service.ValidateFile(file));
        }
    }
}
