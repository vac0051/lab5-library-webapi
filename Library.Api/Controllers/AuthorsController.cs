using Library.Api.Dtos;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthorsController : ControllerBase
{
    private readonly LibraryContext _context;

    public AuthorsController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuthorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await _context.Authors
            .AsNoTracking()
            .Include(author => author.Books)
            .OrderBy(author => author.Name)
            .Select(author => new AuthorDto
            {
                Id = author.Id,
                Name = author.Name,
                BookIds = author.Books.Select(book => book.Id).ToList()
            })
            .ToListAsync(cancellationToken);

        return Ok(authors);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await _context.Authors
            .AsNoTracking()
            .Include(item => item.Books)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (author is null)
        {
            return NotFound();
        }

        return Ok(new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            BookIds = author.Books.Select(book => book.Id).ToList()
        });
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create(AuthorUpsertDto dto, CancellationToken cancellationToken)
    {
        var author = new Author
        {
            Name = dto.Name.Trim()
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        var result = new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            BookIds = []
        };

        return CreatedAtAction(nameof(GetById), new { id = author.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AuthorUpsertDto dto, CancellationToken cancellationToken)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (author is null)
        {
            return NotFound();
        }

        author.Name = dto.Name.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (author is null)
        {
            return NotFound();
        }

        _context.Authors.Remove(author);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
