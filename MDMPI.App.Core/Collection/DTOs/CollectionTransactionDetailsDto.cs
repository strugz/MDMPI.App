using MDMPI.App.Core.CommonOldEntities.DTOs;
using System;

namespace MDMPI.App.Core.Collection.Dtos
{
    public class CollectionTransactionDetailsDto
    {
        public long ID { get; set; }
        public string ReferenceCode { get; set; } = string.Empty;
        public string ClientID { get; set; } = string.Empty;
        public string CollectorID { get; set; } = string.Empty;
        public DateTime CollectionDate { get; set; }
        public string VisitType { get; set; } = string.Empty;
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public string? Status { get; set; }
        public ACCMSTDto? Client { get; set; }
    }
}