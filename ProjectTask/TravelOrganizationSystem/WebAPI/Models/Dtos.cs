using System.ComponentModel.DataAnnotations;

namespace WebAPI.Models
{
    public class ImageUrlUpdateRequest
    {
        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
