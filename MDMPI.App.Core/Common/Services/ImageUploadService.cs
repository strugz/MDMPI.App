using MDMPI.App.Core.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Common.Services
{
    public class ImageUploadService : IImageService
    {
        private readonly IImagePathTypeRepository _repository;
        private readonly ILogger<ImageUploadService> _logger;

        public ImageUploadService(IImagePathTypeRepository repository, ILogger<ImageUploadService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<byte[]?> GetRequestImageAsync(string requestId, string type)
        {
            return await _repository.GetRequestImage(requestId, type);
        }

        public async Task<string?> UploadImageAsync(byte[] imageBytes, string requestId, string type)
        {
            return await _repository.UploadImageAsync(imageBytes, requestId, type);
        }
    }
}
