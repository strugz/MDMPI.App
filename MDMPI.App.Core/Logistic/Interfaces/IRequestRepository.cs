using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.Logistic.DTOs;
using MDMPI.App.Core.Logistic.Entities;

namespace MDMPI.App.Core.Logistic.Interfaces
{
    public interface IRequestRepository
    {
        Task<List<RequestStandardDto>> GetAllRequestsAsync();
        Task<RemarksDto?> GetAllRemarks(string requestid);
        Task<byte[]?> GetRequestProofImage(string requestid);
        Task<byte[]?> GetRequestSignatureImage(string requestid);
        Task<bool> InsertRequest(InsertRequestDto dto);
        Task<bool> UpdateRequest(UpdateRequestDto dto);
        Task<bool> InsertRemarkAndCancelRequestAsync(long requestId, string remarks);
    }
}