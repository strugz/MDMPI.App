using MDMPI.App.Core.Common.DTOs.Item;
using MDMPI.App.Core.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Common.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repository;
        private readonly ILogger<ItemService> _logger;

        public ItemService(IItemRepository repository, ILogger<ItemService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<FetchItemDto>> GetItemsByRequestIdAsync(long requestId)
        {
            return await _repository.GetItemsByRequestIdAsync(requestId);
        }

        public async Task<bool> InsertItemsAsync(long requestId, List<InsertItemDto> items)
        {
            return await _repository.InsertItemsAsync(requestId, items);
        }

        public async Task<bool> UpdateItemsAsync(long requestId, List<UpdateItemDto> items)
        {
            return await _repository.UpdateItemsAsync(requestId, items);
        }
    }
}
