using MDMPI.App.Core.Common.Entities;
using MDMPI.App.Core.Common.Services;
using MDMPI.App.Core.Logistic.Interfaces;
using MDMPI.App.Data.Logistic.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Data.Common.Repositories
{
    public class ImagePathTypeRepository : IImagePathTypeRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<RequestRepository> _logger;

        public ImagePathTypeRepository(AppDbContext db, ILogger<RequestRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        /// <summary>
        /// Gets the image bytes for a request by request ID and type (e.g., "Signature" or "Proof").
        /// Returns the image bytes if found, otherwise null.
        /// </summary>
        public async Task<byte[]?> GetRequestImage(string requestid, string type)
        {
            if (!long.TryParse(requestid, out var id))
            {
                _logger.LogWarning("Invalid RequestID format for GetRequestImage: {RequestId}", requestid);
                return null;
            }

            var imageRecord = await _db.a_tblRequestImagePath
                .AsNoTracking()
                .FirstOrDefaultAsync(img => img.RequestID == id && img.ImageType == type);

            if (imageRecord?.ImagePath == null || !File.Exists(imageRecord.ImagePath))
            {
                _logger.LogWarning("Image file not found for RequestID: {RequestId}, Type: {Type}", requestid, type);
                return null;
            }

            try
            {
                return await File.ReadAllBytesAsync(imageRecord.ImagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading image file for RequestID: {RequestId}, Type: {Type}", requestid, type);
                return null;
            }
        }

        /// <summary>
        /// Save image bytes to disk and persist in database (proof bytes or signature base64).
        /// Returns the file path on success, or null on failure.
        /// </summary>
        public async Task<string?> UploadImageAsync(byte[] imageBytes, string requestId, string type)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                _logger.LogWarning("Empty image bytes provided for request {RequestId}", requestId);
                return null;
            }

            if (!long.TryParse(requestId, out var id))
            {
                _logger.LogWarning("Invalid RequestID format for UploadImage: {RequestId}", requestId);
                return null;
            }

            var directoryPath = ImageService.GetImageDirectory();
            var fileName = type == "Signature" ? $"RequestSignature_{requestId}.png" : $"RequestProof_{requestId}.png";
            var filePath = Path.Combine(directoryPath, fileName);

            try
            {
                // Save to disk using injected service
                ImageService.SaveImageToDirectory(imageBytes, fileName, directoryPath);

                // Persist to DB
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    if (type == "Signature")
                    {
                        var base64 = Convert.ToBase64String(imageBytes);

                        var existingSig = await _db.a_tblRequestImagePath.FirstOrDefaultAsync(s => s.RequestID == id && s.ImageType == type);

                        if (existingSig is null)
                        {
                            _db.a_tblRequestImagePath.Add(new ImagePathModel
                            {
                                RequestID = long.Parse(requestId),
                                ImagePath = filePath,
                                ImageType = type,
                            });
                        }
                        else
                        {
                            existingSig.RequestID = long.Parse(requestId);
                        }
                    }
                    else // Proof
                    {
                        var existingImage = await _db.a_tblRequestImagePath.FirstOrDefaultAsync(i => i.RequestID == id);
                        if (existingImage is null)
                        {
                            _db.a_tblRequestImagePath.Add(new ImagePathModel
                            {
                                RequestID = long.Parse(requestId),
                                ImagePath = filePath,
                                ImageType = type,
                            });
                        }
                        else
                        {
                            existingImage.RequestID = long.Parse(requestId);
                        }
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Uploaded image for RequestID {RequestId} (type: {Type}) to {Path}", requestId, type, filePath);
                    return filePath;
                }
                catch (Exception dbEx)
                {
                    await Helper.RollbackTransactionAsync(transaction, _logger, dbEx, $"Error saving image record for RequestID {requestId}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload image for RequestID {RequestId}", requestId);
                return null;
            }
        }
    }
}
