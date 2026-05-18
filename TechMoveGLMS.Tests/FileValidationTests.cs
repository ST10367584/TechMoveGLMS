using Microsoft.AspNetCore.Http;
using Moq;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests;

public class FileValidationTests
{
    private readonly FileService _service;
    public FileValidationTests()
    {
        var env = new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        env.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<FileService>.Instance;
        _service = new FileService(env.Object, logger);
    }

    private static IFormFile MakeFile(string name, string contentType, long size = 1024)
    {
        var mock = new Mock<IFormFile>();
        mock.Setup(f => f.FileName).Returns(name);
        mock.Setup(f => f.ContentType).Returns(contentType);
        mock.Setup(f => f.Length).Returns(size);
        return mock.Object;
    }

    [Fact]
    public void ValidateFile_ValidPdf_DoesNotThrow() =>
        _service.ValidateFile(MakeFile("doc.pdf", "application/pdf"));

    [Fact]
    public void ValidateFile_Exe_Throws() =>
        Assert.Throws<InvalidOperationException>(() => _service.ValidateFile(MakeFile("virus.exe", "application/octet-stream")));

    [Fact]
    public void ValidateFile_TooLarge_Throws() =>
        Assert.Throws<InvalidOperationException>(() => _service.ValidateFile(MakeFile("big.pdf", "application/pdf", 11 * 1024 * 1024)));

    [Fact]
    public void ValidateFile_EmptyFile_Throws() =>
        Assert.Throws<InvalidOperationException>(() => _service.ValidateFile(MakeFile("empty.pdf", "application/pdf", 0)));
}