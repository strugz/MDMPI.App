using MDMPI.App.Core.Logistic.DTOs.LoseItem;
using MDMPI.App.Core.Logistic.Interfaces;

namespace MDMPI.App.Core.Logistic.Services
{
    public class LoseItemService : ILoseItemService
    {
        private readonly ILoseItemRepository _repository;

        public LoseItemService(ILoseItemRepository repository)
        {
            _repository = repository;
        }

        public Task<bool> PullOutRequestExistsAsync(long requestId)
            => _repository.PullOutRequestExistsAsync(requestId);

        public Task<List<FetchLoseItemDto>> GetByRequestIdAsync(long requestId)
            => _repository.GetByRequestIdAsync(requestId);

        public Task<bool> ReplaceForRequestAsync(long requestId, List<InsertLoseItemDto> items)
            => _repository.ReplaceForRequestAsync(requestId, items);
    }
}
