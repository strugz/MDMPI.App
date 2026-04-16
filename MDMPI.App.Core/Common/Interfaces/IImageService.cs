namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IImageService
    {
        Task<byte[]?> GetRequestImageAsync(string requestId, string type);
        Task<string?> UploadImageAsync(byte[] imageBytes, string requestId, string type);
    }
}
