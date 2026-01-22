using System;
using System.ComponentModel.DataAnnotations;

namespace MDMPI.App.Core.Collection.Dtos
{
    public class CreateCollectionTransactionDetailsDto
    {
        [Required]
        [MaxLength(16)]
        public string ReferenceCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ClientID { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CollectorID { get; set; } = string.Empty;

        [Required]
        public DateTime CollectionDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string VisitType { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Status { get; set; }
    }
}