using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.DTOs.Item;
using MDMPI.App.Core.Common.Interfaces;
using MDMPI.App.Core.Logistic.DTOs.RequestStandard;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Logistic.Services
{
    public class RequestService : IRequestService
    {
        private readonly IRequestRepository _requestRepository;
        private readonly IItemRepository _itemRepository;
        private readonly ILogger<RequestService> _logger;

        public RequestService(
            IRequestRepository requestRepository,
            IItemRepository itemRepository,
            ILogger<RequestService> logger)
        {
            _requestRepository = requestRepository;
            _itemRepository = itemRepository;
            _logger = logger;
        }

        public async Task<List<RequestStandardDto>> GetAllRequestsAsync(RequestQueryDto query)
        {
            _logger.LogInformation("Service: Fetching all requests with filters {@Query}", query);
            return await _requestRepository.GetAllRequestsAsync(query);
        }

        public async Task<RequestStandardDto?> CreateRequestWithItemsAsync(InsertRequestDto dto)
        {
            var inserted = await _requestRepository.InsertRequest(dto);
            if (inserted is null)
            {
                _logger.LogWarning("Service: Failed to insert request");
                return null;
            }

            if (dto.Items is { Count: > 0 })
            {
                await _itemRepository.InsertItemsAsync(long.Parse(inserted.ID!), dto.Items);
            }

            _logger.LogInformation("Service: Created request {RequestID} with {ItemCount} items", inserted.ID, dto.Items?.Count ?? 0);
            return inserted;
        }

        public async Task<bool> UpdateRequestAsync(UpdateRequestDto dto)
        {
            return await _requestRepository.UpdateRequest(dto);
        }

        public async Task<List<RequestStandardHistoryDto>> GetRequestHistoryAsync(long requestId)
        {
            return await _requestRepository.GetAllRequestHistory(requestId);
        }
    }
}
