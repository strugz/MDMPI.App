using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDMPI.App.Core.Common.Entities
{
    public class ImagePathModel
    {
        public long ID { get; set; }
        public long RequestID { get; set; }
        public string? ImagePath { get; set; }
        public string? ImageType { get; set; }
    }
}
