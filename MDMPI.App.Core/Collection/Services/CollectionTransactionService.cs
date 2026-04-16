using MDMPI.App.Core.Collection.Dtos;
using MDMPI.App.Core.Collection.Interfaces;
using Microsoft.Extensions.Logging;

namespace MDMPI.App.Core.Collection.Services
{
    public class CollectionTransactionService : ICollectionTransactionService
    {
        private readonly ICollectionTransactionDetailsRepository _repository;
        private readonly ILogger<CollectionTransactionService> _logger;

        public CollectionTransactionService(ICollectionTransactionDetailsRepository repository, ILogger<CollectionTransactionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<CollectionTransactionDetailsDto>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<CollectionTransactionDetailsDto?> GetByIdAsync(long id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<CollectionTransactionDetailsDto?> CreateAsync(CreateCollectionTransactionDetailsDto dto)
        {
            return await _repository.InsertAsync(dto);
        }

        public async Task<CollectionTransactionDetailsDto?> UpdateAsync(UpdateCollectionTransactionDetailsDto dto)
        {
            return await _repository.UpdateAsync(dto);
        }
    }
}
