using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/gallery-items")]
[Authorize] // JWT protected
public class GalleryItemsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public GalleryItemsController(AppDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem([FromForm] GalleryItemCreateDto dto)
    {
        if (dto.Image == null || dto.Image.Length == 0)
            return BadRequest("Image is required.");

        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsPath);

        var fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
        var filePath = Path.Combine(uploadsPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await dto.Image.CopyToAsync(stream);
        }

        var item = new GalleryItem
        {
            Name = dto.Name,
            Description = dto.Description,
            Image = Path.Combine("uploads", fileName).Replace("\\", "/")
        };

        _context.GalleryItems.Add(item);
        await _context.SaveChangesAsync();

        return Ok(item);
    }

    [HttpGet]
    public async Task<IActionResult> GetItems(
    [FromQuery] string search = "",
    [FromQuery] int limit = 10,
    [FromQuery] int page = 1)
    {
        if (limit <= 0) limit = 10;
        if (page <= 0) page = 1;

        var query = _context.GalleryItems.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(i =>
                i.Name.Contains(search) || i.Description.Contains(search));
        }

        // Default order by Id ascending
        query = query.OrderBy(i => i.Id);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)limit);

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return Ok(new
        {
            items,
            page,
            totalPages
        });
    }

}
