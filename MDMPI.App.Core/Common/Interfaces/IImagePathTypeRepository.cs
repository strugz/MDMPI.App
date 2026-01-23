namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IImagePathTypeRepository
    {
        Task<byte[]?> GetRequestImage(string requestid, string type);
        Task<string?> UploadImageAsync(byte[] imageBytes, string requestId, string type);
    }
}