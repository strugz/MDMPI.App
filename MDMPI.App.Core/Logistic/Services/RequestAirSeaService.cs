using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs.RequestAirSea;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Logistic.Services
{
    public class RequestAirSeaService : IRequestAirSeaService
    {
        private readonly IRequestAirSeaRepository _repository;
        private readonly ILogger<RequestAirSeaService> _logger;

        public RequestAirSeaService(IRequestAirSeaRepository repository, ILogger<RequestAirSeaService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<RequestAirSeaDto>> GetAllAsync(RequestQueryDto query)
        {
            return await _repository.GetAllAsync(query);
        }

        public async Task<RequestAirSeaDto?> InsertAsync(InsertRequestAirSeaDto dto)
        {
            var result = await _repository.InsertAsync(dto);
            if (result is null)
                _logger.LogWarning("Service: Failed to insert air/sea request");
            return result;
        }

        public async Task<bool> UpdateAsync(UpdateRequestAirSeaDto dto)
        {
            return await _repository.UpdateAsync(dto);
        }
    }
}
