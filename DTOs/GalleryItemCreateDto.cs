using System.ComponentModel.DataAnnotations;
public class GalleryItemCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public IFormFile Image { get; set; } = null!;
}
