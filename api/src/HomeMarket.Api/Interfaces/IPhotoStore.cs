namespace HomeMarket.Api.Interfaces
{
    public enum PhotoOutcome
    {
        Saved,
        NotAnImage,
        TooLarge,
    }

    // Uploaded photos live on disk under a name the server chooses.
    public interface IPhotoStore
    {
        Task<(PhotoOutcome Outcome, string FileName)> SaveAsync(IFormFile file);
        bool Exists(string fileName);
        string UrlFor(string photo);
    }
}
