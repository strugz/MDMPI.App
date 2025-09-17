using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.Entities
{
    public class MobileModel
    {
        [Key]
        public long MobileID { get; set; }
        public string? MobileName { get; set; }

    }
}
