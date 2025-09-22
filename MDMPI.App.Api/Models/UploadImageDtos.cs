using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MDMPI.App.Api.Models
{
    public class UploadImageRequestDto
    {
        [Required]
        public IFormFile? Image { get; set; }
        [Required]
        public string? RequestID { get; set; }
        [Required]
        public string? Type { get; set; }
    }
}
