using MDMPI.App.Core.Common.DTOs.Item;

namespace MDMPI.App.Core.Common.Interfaces
{
    public interface IItemService
    {
        Task<List<FetchItemDto>> GetItemsByRequestIdAsync(long requestId);
        Task<bool> InsertItemsAsync(long requestId, List<InsertItemDto> items);
        Task<bool> UpdateItemsAsync(long requestId, List<UpdateItemDto> items);
    }
}
