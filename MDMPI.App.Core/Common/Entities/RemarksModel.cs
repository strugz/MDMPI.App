using MDMPI.App.Core.Common.DTOs;
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
    public class RemarksModel
    {
        [Key]
        public long? RequestID { get; set; }
        public string? Remarks { get; set; }
        public DateTime? Date { get; set; }
    }
}
