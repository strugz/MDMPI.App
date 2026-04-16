using MDMPI.App.Core.Collection.Dtos;

namespace MDMPI.App.Core.Collection.Interfaces
{
    public interface ICollectionTransactionService
    {
        Task<List<CollectionTransactionDetailsDto>> GetAllAsync();
        Task<CollectionTransactionDetailsDto?> GetByIdAsync(long id);
        Task<CollectionTransactionDetailsDto?> CreateAsync(CreateCollectionTransactionDetailsDto dto);
        Task<CollectionTransactionDetailsDto?> UpdateAsync(UpdateCollectionTransactionDetailsDto dto);
    }
}
