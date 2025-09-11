using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GsC.API.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Menu")]
        public int MenuId { get; set; }

        [Required]
        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Menu Menu { get; set; } = null!;
        public virtual Article Article { get; set; } = null!;
    }
}