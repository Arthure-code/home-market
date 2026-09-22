using System.Security.Cryptography;
using Ardalis.Result;
using HomeMarket.Api.Interfaces;

namespace HomeMarket.Api.Services
{
    // Uploads land in one folder under a random name with the extension
    // the image's own bytes call for, never the name the client sent.
    public class PhotoStore : IPhotoStore
    {
        private const string NotAnImage = "That file is not a JPEG, PNG, GIF or WebP image.";
        public const long MaxBytes = 5 * 1024 * 1024;

        private static readonly Dictionary<string, byte[]> Signatures = new()
        {
            [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
            [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47 },
            [".gif"] = new byte[] { 0x47, 0x49, 0x46, 0x38 },
            [".webp"] = new byte[] { 0x52, 0x49, 0x46, 0x46 },
        };

        private readonly string _folder;
        private readonly string _baseUrl;

        public PhotoStore(IWebHostEnvironment environment, IConfiguration configuration)
        {
            _folder = Path.Combine(environment.ContentRootPath, "images");
            _baseUrl = (configuration["BaseUrl"] ?? string.Empty).TrimEnd('/');
            Directory.CreateDirectory(_folder);
        }

        public async Task<Result<string>> SaveAsync(IFormFile file)
        {
            if (file.Length == 0 || file.Length > MaxBytes) return Invalid("Photos must be 5 MB or less.");

            var header = new byte[4];
            await using (var probe = file.OpenReadStream())
            {
                var read = await probe.ReadAsync(header);
                if (read < header.Length) return Invalid(NotAnImage);
            }

            var extension = Signatures.FirstOrDefault(s => header.Take(s.Value.Length).SequenceEqual(s.Value)).Key;
            if (extension is null) return Invalid(NotAnImage);

            var name = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant() + extension;
            await using (var target = File.Create(Path.Combine(_folder, name)))
            {
                await file.CopyToAsync(target);
            }
            return Result<string>.Success(name);
        }

        private static Result<string> Invalid(string reason)
        {
            return Result<string>.Invalid(new ValidationError("photo", reason));
        }

        // Only a name this store produced, and that is still on disk.
        public bool Exists(string fileName)
        {
            return fileName.Length > 0
                && fileName.All(c => char.IsAsciiHexDigitLower(c) || c == '.' || char.IsAsciiLetterLower(c))
                && !fileName.Contains("..")
                && File.Exists(Path.Combine(_folder, fileName));
        }

        // A seeded product carries a full address; an upload carries only
        // its file name, served by the API itself.
        public string UrlFor(string photo)
        {
            if (string.IsNullOrEmpty(photo)) return string.Empty;
            if (photo.StartsWith("https://", StringComparison.Ordinal)) return photo;
            return $"{_baseUrl}/images/{photo}";
        }
    }
}
