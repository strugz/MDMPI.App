using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestBackload;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Logistic.Services
{
    public class RequestBackloadService : IRequestBackloadService
    {
        private readonly IRequestBackloadRepository _repository;
        private readonly ILogger<RequestBackloadService> _logger;

        public RequestBackloadService(IRequestBackloadRepository repository, ILogger<RequestBackloadService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<RequestBackloadDto>> GetAllAsync(RequestQueryDto query)
        {
            return await _repository.GetAllAsync(query);
        }

        public async Task<RequestBackloadDto?> InsertAsync(InsertRequestBackloadDto dto)
        {
            var result = await _repository.InsertAsync(dto);
            if (result is null)
                _logger.LogWarning("Service: Failed to insert backload request");
            return result;
        }
    }
}
