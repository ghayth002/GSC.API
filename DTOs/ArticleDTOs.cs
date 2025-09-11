using System.ComponentModel.DataAnnotations;
using GsC.API.Models;

namespace GsC.API.DTOs
{
    public class ArticleDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TypeArticle Type { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public string? Supplier { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateArticleDto
    {
        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public TypeArticle Type { get; set; }

        [Required]
        [StringLength(50)]
        public string Unit { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateArticleDto
    {
        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public TypeArticle? Type { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        public decimal? UnitPrice { get; set; }

        [StringLength(100)]
        public string? Supplier { get; set; }

        public bool? IsActive { get; set; }
    }

    public class ArticleDetailsDto : ArticleDto
    {
        public List<MenuItemDto> MenuItems { get; set; } = new();
        public List<PlanHebergementArticleDto> PlanHebergementArticles { get; set; } = new();
    }
}
