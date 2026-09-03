using MDMPI.App.Core.Logistic.DTOs.LoseItem;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface ILoseItemRepository
    {
        /// <summary>
        /// Returns true when the request id exists in a_tblRequestPullOutReturnPickUp.
        /// </summary>
        Task<bool> PullOutRequestExistsAsync(long requestId);

        Task<List<FetchLoseItemDto>> GetByRequestIdAsync(long requestId);

        /// <summary>
        /// Replaces the lost-item set for a request (delete existing rows, insert the new set).
        /// </summary>
        Task<bool> ReplaceForRequestAsync(long requestId, List<InsertLoseItemDto> items);
    }
}
