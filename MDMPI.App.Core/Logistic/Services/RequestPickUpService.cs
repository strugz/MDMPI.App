using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestPickUp;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Logistic.Services
{
    public class RequestPickUpService : IRequestPickUpService
    {
        private readonly IRequestPickUpRepository _repository;
        private readonly ILogger<RequestPickUpService> _logger;

        public RequestPickUpService(IRequestPickUpRepository repository, ILogger<RequestPickUpService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<RequestPickUpDto>> GetAllAsync(RequestQueryDto query)
        {
            return await _repository.GetAllAsync(query);
        }

        public async Task<RequestPickUpDto?> InsertAsync(InsertRequestPickUpDto dto)
        {
            var result = await _repository.InsertAsync(dto);
            if (result is null)
                _logger.LogWarning("Service: Failed to insert pickup request");
            return result;
        }

        public async Task<bool> UpdateAsync(UpdateRequestPickUpDto dto)
        {
            return await _repository.UpdateAsync(dto);
        }
    }
}
