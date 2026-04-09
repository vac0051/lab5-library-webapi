using Library.Api.Dtos;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController : ControllerBase
{
    private readonly LibraryContext _context;

    public BooksController(LibraryContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _context.Books
            .AsNoTracking()
            .Include(book => book.Author)
            .Include(book => book.Genres)
            .OrderBy(book => book.Title)
            .ToListAsync(cancellationToken);

        return Ok(books.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .AsNoTracking()
            .Include(item => item.Author)
            .Include(item => item.Genres)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(book));
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(BookUpsertDto dto, CancellationToken cancellationToken)
    {
        var author = await _context.Authors.FirstOrDefaultAsync(item => item.Id == dto.AuthorId, cancellationToken);
        if (author is null)
        {
            return BadRequest("Author does not exist.");
        }

        var genres = await _context.Genres
            .Where(genre => dto.GenreIds.Contains(genre.Id))
            .ToListAsync(cancellationToken);

        if (genres.Count != dto.GenreIds.Count)
        {
            return BadRequest("One or more genres do not exist.");
        }

        var book = new Book
        {
            Title = dto.Title.Trim(),
            PublicationYear = dto.PublicationYear,
            AuthorId = author.Id,
            Genres = genres
        };

        _context.Books.Add(book);
        await _context.SaveChangesAsync(cancellationToken);

        await _context.Entry(book).Reference(item => item.Author).LoadAsync(cancellationToken);
        await _context.Entry(book).Collection(item => item.Genres).LoadAsync(cancellationToken);

        var result = MapToDto(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookUpsertDto dto, CancellationToken cancellationToken)
    {
        var book = await _context.Books
            .Include(item => item.Genres)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        var authorExists = await _context.Authors.AnyAsync(item => item.Id == dto.AuthorId, cancellationToken);
        if (!authorExists)
        {
            return BadRequest("Author does not exist.");
        }

        var genres = await _context.Genres
            .Where(genre => dto.GenreIds.Contains(genre.Id))
            .ToListAsync(cancellationToken);

        if (genres.Count != dto.GenreIds.Count)
        {
            return BadRequest("One or more genres do not exist.");
        }

        book.Title = dto.Title.Trim();
        book.PublicationYear = dto.PublicationYear;
        book.AuthorId = dto.AuthorId;
        book.Genres = genres;

        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static BookDto MapToDto(Book book)
    {
        return new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            PublicationYear = book.PublicationYear,
            AuthorId = book.AuthorId,
            AuthorName = book.Author?.Name ?? string.Empty,
            Genres = book.Genres.Select(genre => genre.Name).OrderBy(name => name).ToList()
        };
    }
}
