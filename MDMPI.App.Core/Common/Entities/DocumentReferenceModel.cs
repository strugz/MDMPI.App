using System.ComponentModel.DataAnnotations;

namespace MDMPI.App.Core.Common.Entities
{
    public class DocumentReferenceModel
    {
        public long ID { get; set; }
        public long? RequestID { get; set; }
        public string? Reference { get; set; }
    }
}
