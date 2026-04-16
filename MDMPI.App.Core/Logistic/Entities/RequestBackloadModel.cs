using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MDMPI.App.Core.Logistic.Entities
{
    public class RequestBackloadModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BackLoadID { get; set; }

        public long? RequestID { get; set; }

        public string? Remarks { get; set; }

        public DateTime? DateReported { get; set; }
        
        public DateOnly? DeliveryDate { get; set; }

        [ForeignKey("RequestID")]
        public RequestStandardModel? Request { get; set; }
    }
}
