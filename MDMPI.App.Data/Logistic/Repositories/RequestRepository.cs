using MDMPI.App.Core.Common.DTOs;
using MDMPI.App.Core.CommonOldEntities.DTOs;
using MDMPI.App.Core.Logistic.DTOs;
using MDMPI.App.Core.Logistic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MDMPI.App.Data.Logistic.Repositories
{
    public class RequestRepository : IRequestRepository
    {
        private readonly AppDbContext _db;

        public RequestRepository(AppDbContext db) => _db = db;

        public async Task<List<RequestStandardDto>> GetAllRequestsAsync()
        {
            var dto = await _db.a_tblRequest.Select(r => new RequestStandardDto
            {
                ID = r.RequestID,
                ClientID = r.RequestClientID,
                ShippingMethod = r.RequestShippingMethod,
                DeliveryTerms = r.RequestDeliveryTerms,
                DeliveryDate = r.RequestDeliveryDate,
                Preference = r.RequestPreference,
                Status = r.RequestStatus,
                RequestBy = r.RequestBy,
                CreatedBy = r.RequestCreatedBy,
                CreatedAt = r.RequestCreatedAt,
                ItemPreparedBy = r.RequestItemPreparedBy,
                DeliveredBy = r.RequestDeliveredBy,
                ItemPreparedAt = r.RequestItemPreparedAt,
                ItemPreparedEndAt = r.RequestItemPreparedEndAt,
                DeliveredAt = r.RequestDeliveredAt,
                DeliveredEndAt = r.RequestDeliveredEndAt,
                MobileID = r.MobileID,
                MobileName = r.Mobile!.MobileName,
                Helper = r.RequestDriverHelper,
                Receiver = r.Receiver,
                TripTicketNumber = r.RequestTripTicketNumber,
                DocumentReference = r.DocumenRDocumentReference!
                .Select(dr => dr.Reference)
                .ToList(),
                Client = r.Client == null ? null : new ACCMSTDto
                {
                    ACCMID = r.Client.ACCMID,
                    ACCMSC = r.Client.ACCMSC,
                    ACCMNM = r.Client.ACCMNM,
                    ACCMBC = r.Client.ACCMBC,
                    ACCMAD = r.Client.ACCMAD,
                    ACCMPH = r.Client.ACCMPH,
                    ACCMEM = r.Client.ACCMEM,
                    ACCMWS = r.Client.ACCMWS,
                    ACCSTS = r.Client.ACCSTS,
                    ACCOWN = r.Client.ACCOWN
                }
            }).ToListAsync();
            return dto;
        }

        public async Task<RemarksDto> GetAllRemarks(string requestid)
        {
            var dto = await _db.a_tblRequestRemarks.Where(r => r.RequestID.ToString() == requestid).Select(r => new RemarksDto
            {
                RequestID = r.RequestID,
                Remarks = r.Remarks,
                Date = r.Date.HasValue
                    ? r.Date.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : null
            }).FirstOrDefaultAsync();
            return dto!;
        }

        public async Task<byte[]?> GetRequestProofImage(string requestid)
        {
            var dto = await _db.a_tblRequestImage.Where(r => r.RequestID.ToString() == requestid).Select(r => new ImageDto
            {
                RequestID = r.RequestID,
                Image = r.RequestImage
            }).FirstOrDefaultAsync();
            return dto!.Image;
        }

        public async Task<byte[]?> GetRequestSignatureImage(string requestid)
        {
            var dto = await _db.a_tblRequestReceiverSignature.Where(r => r.RequestID.ToString() == requestid).Select(r => new SignatureDto
            {
                RequestID = r.RequestID,
                Image = r.RequestReceiverSignature
            }).FirstOrDefaultAsync();

            return Convert.FromBase64String(dto!.Image!);
        }


    }
}
