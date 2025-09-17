using MDMPI.App.Core.Logistic.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.Entities
{
    public class SignatureModel
    {
        [Key]
        public long? RequestID { get; set; }
        public string? RequestReceiverSignature { get; set; }

        [ForeignKey(nameof(RequestID))]
        public RequestStandardModel? Request { get; set; }
    }
}
