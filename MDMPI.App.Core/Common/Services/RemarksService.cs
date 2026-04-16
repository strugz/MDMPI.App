using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Common.Services
{
    public class RemarksService : IRemarksService
    {
        private readonly IRequestRemarksRepository _repository;
        private readonly ILogger<RemarksService> _logger;

        public RemarksService(IRequestRemarksRepository repository, ILogger<RemarksService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<RemarksDto?> GetAllRemarks(long requestId)
        {
            return await _repository.GetAllRemarks(requestId);
        }

        public async Task<bool> CancelStandardDeliveryAsync(long requestId, string user, string remarks)
        {
            return await _repository.InsertRemarkAndCancelRequestForStandardDeliveryAsync(requestId, user, remarks);
        }

        public async Task<bool> CancelPullOutReturnPickUpAsync(long requestId, string user, string remarks)
        {
            return await _repository.InsertRemarkAndCancelRequestForPullOutReturnPickUp(requestId, user, remarks);
        }

        public async Task<bool> CancelAirSeaAsync(long requestId, string user, string remarks)
        {
            return await _repository.InsertRemarkAndCancelRequestForAirSea(requestId, user, remarks);
        }

        public async Task<bool> CancelPickUpAsync(long requestId, string user, string remarks)
        {
            return await _repository.InsertRemarkAndCancelRequestForPickUp(requestId, user, remarks);
        }
    }
}
