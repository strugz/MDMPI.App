using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Common.Interfaces;

namespace MDMPI.App.Core.Common.Services
{
    public class MobileService : IMobileService
    {
        private readonly IMobileRepository _repository;

        public MobileService(IMobileRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MobileDto>> GetAllMobilesAsync()
        {
            return await _repository.GetAllMobilesAsync();
        }
    }
}
