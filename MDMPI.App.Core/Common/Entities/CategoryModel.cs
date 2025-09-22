using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.Entities
{
    public class CategoryModel
    {
        [Key]
        public long? ID { get; set; }
        public string? Category { get; set; }
        public string? Type { get; set; }
    }
}
