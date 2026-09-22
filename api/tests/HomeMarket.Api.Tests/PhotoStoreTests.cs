using Ardalis.Result;
using HomeMarket.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace HomeMarket.Api.Tests
{
    public class PhotoStoreTests
    {
        private static readonly byte[] PngHeader = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

        // A store rooted in a fresh folder under the temp directory, so
        // nothing a test writes meets what another wrote.
        private static (PhotoStore Store, string Root) StoreInTemp(string baseUrl = "http://api")
        {
            var root = Path.Combine(Path.GetTempPath(), "home-market-tests", Guid.NewGuid().ToString("N"));
            var environment = new Mock<IWebHostEnvironment>();
            environment.SetupGet(e => e.ContentRootPath).Returns(root);
            var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["BaseUrl"] = baseUrl }).Build();
            return (new PhotoStore(environment.Object, configuration), root);
        }

        private static IFormFile FileOf(byte[] bytes, string name = "upload.bin")
        {
            return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "photo", name);
        }

        [Fact]
        public async Task SaveAsync_Png_StoresItUnderARandomNameWithTheExtensionOfItsBytes()
        {
            // Given
            var (store, root) = StoreInTemp();
            var bytes = PngHeader.Concat(new byte[100]).ToArray();

            // When
            var result = await store.SaveAsync(FileOf(bytes, "holiday.JPG"));

            // Then
            Assert.True(result.IsSuccess);
            Assert.EndsWith(".png", result.Value);
            Assert.Equal(36, result.Value.Length);
            Assert.DoesNotContain("holiday", result.Value);
            Assert.Equal(bytes, await File.ReadAllBytesAsync(Path.Combine(root, "images", result.Value)));
        }

        [Fact]
        public async Task SaveAsync_JpegGifAndWebp_AreRecognisedByTheirBytes()
        {
            // Given
            var (store, _) = StoreInTemp();
            var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0, 0, 0 };
            var gif = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0, 0 };
            var webp = new byte[] { 0x52, 0x49, 0x46, 0x46, 0, 0, 0, 0 };

            // When
            var savedJpeg = await store.SaveAsync(FileOf(jpeg));
            var savedGif = await store.SaveAsync(FileOf(gif));
            var savedWebp = await store.SaveAsync(FileOf(webp));

            // Then
            Assert.EndsWith(".jpg", savedJpeg.Value);
            Assert.EndsWith(".gif", savedGif.Value);
            Assert.EndsWith(".webp", savedWebp.Value);
        }

        [Fact]
        public async Task SaveAsync_FileThatIsNotAnImage_IsInvalidAndStoresNothing()
        {
            // Given
            var (store, root) = StoreInTemp();
            var text = "<script>alert(1)</script>"u8.ToArray();

            // When
            var result = await store.SaveAsync(FileOf(text, "photo.png"));

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
            Assert.Equal("photo", Assert.Single(result.ValidationErrors).Identifier);
            Assert.Empty(Directory.GetFiles(Path.Combine(root, "images")));
        }

        [Fact]
        public async Task SaveAsync_FileShorterThanAHeader_IsInvalid()
        {
            // Given
            var (store, _) = StoreInTemp();

            // When
            var result = await store.SaveAsync(FileOf(new byte[] { 0x89, 0x50 }));

            // Then
            Assert.Equal(ResultStatus.Invalid, result.Status);
        }

        [Fact]
        public async Task SaveAsync_EmptyOrOversizedFile_IsInvalid()
        {
            // Given
            var (store, _) = StoreInTemp();
            var oversized = new Mock<IFormFile>();
            oversized.SetupGet(f => f.Length).Returns(PhotoStore.MaxBytes + 1);

            // When
            var empty = await store.SaveAsync(FileOf(Array.Empty<byte>()));
            var tooLarge = await store.SaveAsync(oversized.Object);

            // Then
            Assert.Equal(ResultStatus.Invalid, empty.Status);
            Assert.Equal(ResultStatus.Invalid, tooLarge.Status);
            Assert.Equal("Photos must be 5 MB or less.", Assert.Single(tooLarge.ValidationErrors).ErrorMessage);
        }

        [Fact]
        public async Task Exists_OnlyForANameThisStoreProducedAndStillHolds()
        {
            // Given
            var (store, root) = StoreInTemp();
            var saved = await store.SaveAsync(FileOf(PngHeader));
            File.WriteAllText(Path.Combine(root, "secret.txt"), "not a photo");

            // When
            var ownName = store.Exists(saved.Value);
            var unknownName = store.Exists("0123456789abcdef0123456789abcdef.png");
            var upwards = store.Exists("../secret.txt");
            var empty = store.Exists(string.Empty);

            // Then
            Assert.True(ownName);
            Assert.False(unknownName);
            Assert.False(upwards);
            Assert.False(empty);
        }

        [Fact]
        public void UrlFor_LinkedPhotoUploadedPhotoAndNone_AnswerAccordingly()
        {
            // Given
            var (store, _) = StoreInTemp("http://api/");

            // When
            var linked = store.UrlFor("https://images.example.com/photo-1");
            var uploaded = store.UrlFor("abc.png");
            var none = store.UrlFor(string.Empty);

            // Then
            Assert.Equal("https://images.example.com/photo-1", linked);
            Assert.Equal("http://api/images/abc.png", uploaded);
            Assert.Empty(none);
        }
    }
}
