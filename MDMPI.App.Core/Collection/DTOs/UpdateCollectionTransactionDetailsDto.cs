using System;
using System.ComponentModel.DataAnnotations;

namespace MDMPI.App.Core.Collection.Dtos
{
    public class UpdateCollectionTransactionDetailsDto
    {
        [Required]
        public long ID { get; set; }

        [Required]
        [MaxLength(16)]
        public string ReferenceCode { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Bank { get; set; }

        [MaxLength(25)]
        public string? CheckNo { get; set; }

        public DateTime? CheckDate { get; set; }

        public decimal? Amount { get; set; }

        [MaxLength(25)]
        public string? SalesInvoiceReferenceForPayment { get; set; }

        public string? SummaryOfVisit { get; set; }

        [MaxLength(20)]
        public string? Status { get; set; }


    }
}