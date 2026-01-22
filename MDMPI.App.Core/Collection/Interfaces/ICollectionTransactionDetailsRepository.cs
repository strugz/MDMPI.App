using MDMPI.App.Core.Collection.Dtos;

namespace MDMPI.App.Core.Collection.Interfaces
{
    public interface ICollectionTransactionDetailsRepository
    {
        Task<List<CollectionTransactionDetailsDto>> GetAllAsync();
        Task<CollectionTransactionDetailsDto?> GetByIdAsync(long id);
        Task<CollectionTransactionDetailsDto?> InsertAsync(CreateCollectionTransactionDetailsDto dto);
        Task<CollectionTransactionDetailsDto?> UpdateAsync(UpdateCollectionTransactionDetailsDto dto);
    }
}
