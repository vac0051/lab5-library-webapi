using Library.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class GenresController : ControllerBase
{
    private readonly LibraryContext _context;

    public GenresController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<object>>> GetAll(CancellationToken cancellationToken)
    {
        var genres = await _context.Genres
            .AsNoTracking()
            .OrderBy(genre => genre.Name)
            .Select(genre => new { genre.Id, genre.Name })
            .ToListAsync(cancellationToken);

        return Ok(genres);
    }
}
