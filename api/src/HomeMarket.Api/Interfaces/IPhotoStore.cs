using Ardalis.Result;

namespace HomeMarket.Api.Interfaces
{
    // Uploaded photos live on disk under a name the server chooses. A
    // file that is not an image, or too large, is Invalid.
    public interface IPhotoStore
    {
        Task<Result<string>> SaveAsync(IFormFile file);
        bool Exists(string fileName);
        string UrlFor(string photo);
    }
}
