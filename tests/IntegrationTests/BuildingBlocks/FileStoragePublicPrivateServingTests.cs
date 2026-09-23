using System.Net;
using GenclikMerkezi.IntegrationTests.Identity;
using GenclikMerkezi.SharedKernel.Abstractions;
using GenclikMerkezi.SharedKernel.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace GenclikMerkezi.IntegrationTests.BuildingBlocks;

// ADR-019 Ek (ADR-024 Faz 0 Görev 4): verifies Host's static file wiring end to end - a
// public-category file is actually servable at PublicRequestPath with the expected headers, and a
// private-category file cannot be retrieved from either the public path or the legacy /uploads
// path, since no static mapping exists for the private root at all.
public class FileStoragePublicPrivateServingTests(CustomWebApplicationFactory factory)
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static FileValidationPolicy CreatePolicy() =>
        FileValidationPolicy.Create(["jpg", "png"], ["image/jpeg", "image/png"], maxSizeInBytes: 2 * 1024 * 1024);

    [Fact]
    public async Task PublicCategoryFile_IsServedUnderPublicRequestPath_WithExpectedHeaders()
    {
        using var scope = factory.Services.CreateScope();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        var originalBytes = "public-file-content"u8.ToArray();
        using var content = new MemoryStream(originalBytes);

        var uploaded = await fileStorageService.UploadAsync(
            content, "logo.png", "image/png", FileCategory.WebsiteImage, "Test", Guid.NewGuid(), CreatePolicy());
        Assert.True(uploaded.IsSuccess);

        try
        {
            var response = await _client.GetAsync($"/webuploads/{uploaded.Value.FileKey}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(originalBytes, await response.Content.ReadAsByteArrayAsync());
            Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
            Assert.Contains("immutable", response.Headers.CacheControl!.ToString());
        }
        finally
        {
            await fileStorageService.DeleteAsync(uploaded.Value.FileKey);
        }
    }

    [Fact]
    public async Task PrivateCategoryFile_IsNotReachableFromPublicOrLegacyUploadsPath()
    {
        using var scope = factory.Services.CreateScope();
        var fileStorageService = scope.ServiceProvider.GetRequiredService<IFileStorageService>();
        using var content = new MemoryStream("private-file-content"u8.ToArray());

        var uploaded = await fileStorageService.UploadAsync(
            content, "photo.jpg", "image/jpeg", FileCategory.CandidatePhoto, "Test", Guid.NewGuid(), CreatePolicy());
        Assert.True(uploaded.IsSuccess);

        try
        {
            var fromPublicPath = await _client.GetAsync($"/webuploads/{uploaded.Value.FileKey}");
            var fromLegacyUploadsPath = await _client.GetAsync($"/uploads/{uploaded.Value.FileKey}");

            Assert.Equal(HttpStatusCode.NotFound, fromPublicPath.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, fromLegacyUploadsPath.StatusCode);
        }
        finally
        {
            await fileStorageService.DeleteAsync(uploaded.Value.FileKey);
        }
    }
}
