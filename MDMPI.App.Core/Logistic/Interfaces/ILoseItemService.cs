using MDMPI.App.Core.Logistic.DTOs.LoseItem;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface ILoseItemService
    {
        Task<bool> PullOutRequestExistsAsync(long requestId);

        Task<List<FetchLoseItemDto>> GetByRequestIdAsync(long requestId);

        Task<bool> ReplaceForRequestAsync(long requestId, List<InsertLoseItemDto> items);
    }
}
