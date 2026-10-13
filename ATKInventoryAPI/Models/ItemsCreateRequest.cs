using System.ComponentModel.DataAnnotations;

namespace ATKInventoryAPI.Models
{
    public class ItemsCreateRequest
    {
        [Required]
        public string Itemcode { get; set; } = string.Empty;
        [Required]
        public string Itemname { get; set; } = string.Empty;
        [Required]
        public string Category { get; set; } = string.Empty;
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Stock { get; set; }

    }
}
