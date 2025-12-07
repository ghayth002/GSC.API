using System.ComponentModel.DataAnnotations;

namespace GsC.API.DTOs
{
    public class MenuDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Title => Name;
        public string? Description { get; set; }
        public string TypePassager { get; set; } = string.Empty;
        public string? Season { get; set; }
        public string? Zone { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateMenuDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        public string TypePassager { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Season { get; set; }

        [StringLength(50)]
        public string? Zone { get; set; }

        public bool IsActive { get; set; } = true;

        public List<CreateMenuItemDto> MenuItems { get; set; } = new();
    }

    public class UpdateMenuDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }

        [StringLength(20)]
        public string? Season { get; set; }

        [StringLength(50)]
        public string? Zone { get; set; }

        public bool? IsActive { get; set; }
    }

    public class MenuDetailsDto : MenuDto
    {
        public List<MenuItemDto> MenuItems { get; set; } = new();
        public List<PlanHebergementDto> PlansHebergement { get; set; } = new();
    }

    public class MenuItemDto
    {
        public int Id { get; set; }
        public int MenuId { get; set; }
        public int ArticleId { get; set; }
        public int Quantity { get; set; }
        public string? TypePassager { get; set; }
        public DateTime CreatedAt { get; set; }
        public ArticleDto Article { get; set; } = new();
    }

    public class CreateMenuItemDto
    {
        [Required]
        public int ArticleId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }
    }

    public class UpdateMenuItemDto
    {
        public int? Quantity { get; set; }

        [StringLength(50)]
        public string? TypePassager { get; set; }
    }
}