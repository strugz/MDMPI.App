using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Logistic.Services
{
    public class RequestPullOutReturnPickUpService : IRequestPullOutReturnPickUpService
    {
        private readonly IRequestPullOutReturnPickUpRepository _repository;
        private readonly ILogger<RequestPullOutReturnPickUpService> _logger;

        public RequestPullOutReturnPickUpService(IRequestPullOutReturnPickUpRepository repository, ILogger<RequestPullOutReturnPickUpService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<RequestPullOutReturnPickUpDto>> GetAllAsync(RequestQueryDto query)
        {
            return await _repository.GetAllAsync(query);
        }

        public async Task<RequestPullOutReturnPickUpDto?> InsertAsync(InsertRequestPullOutReturnPickUpDto dto)
        {
            var result = await _repository.InsertAsync(dto);
            if (result is null)
                _logger.LogWarning("Service: Failed to insert pull-out/return/pickup request");
            return result;
        }

        public async Task<bool> UpdateAsync(UpdateRequestPullOutReturnPickUpDto dto)
        {
            return await _repository.UpdateAsync(dto);
        }
    }
}
