using System;

namespace MDMPI.App.Core.Logistic.DTOs.RequestPullOutReturnPickUp
{
    public class InsertRequestPullOutReturnPickUpDto
    {
        public string? ClientID { get; set; }
        public string? ClientContactPerson { get; set; }
        public long? FormCategoryID { get; set; }
        public long? ItemCategoryID { get; set; }
        public string? SlipNo { get; set; }
        public string? IRRFNumber { get; set; }
        public DateOnly? IRRFDate { get; set; }
        public string? ReasonForReturn { get; set; }
        public List<string>? DocumentReference { get; set; }
        public DateOnly? PullOutDate { get; set; }
        public string? RequestStatus { get; set; }
        public string? CreatedBy { get; set; }
        public string? RequestedBy { get; set; }

    }
}
