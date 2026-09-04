using System.ComponentModel.DataAnnotations;

namespace MDMPI.App.Core.Common.Entities
{
    public class DLRMSTModel
    {
        [Key]
        public string? DLRMID { get; set; }        // Dealer ID
        public string? DLRMSC { get; set; }        // Short Code
        public string? DLRMNM { get; set; }        // Name
        public string? DLRMBC { get; set; }        // Business Code
        public string? DLRMAD { get; set; }        // Address
        public string? DLRMPH { get; set; }        // Phone
        public string? DLRMEM { get; set; }        // Email
        public string? DLRMWS { get; set; }        // Website
        public string? DLRSTS { get; set; }        // Status
        public string? DLROWN { get; set; }        // Owner
    }
}
