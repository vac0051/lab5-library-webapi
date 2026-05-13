using Library.Api.Dtos;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BooksController : ControllerBase
{
    private readonly BookRepository _bookRepository;
    private readonly AuthorRepository _authorRepository;
    private readonly GenreRepository _genreRepository;

    public BooksController(BookRepository bookRepository, AuthorRepository authorRepository, GenreRepository genreRepository)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
        _genreRepository = genreRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BookDto>>> GetAll(CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        return Ok(books.Select(MapToDto).ToList());
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(MapToDto(book));
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> Create(BookUpsertDto dto, CancellationToken cancellationToken)
    {
        var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId, cancellationToken);
        if (!authorExists)
        {
            return BadRequest("Author does not exist.");
        }

        var genres = await _genreRepository.GetByIdsAsync(dto.GenreIds, cancellationToken);

        if (genres.Count != dto.GenreIds.Count)
        {
            return BadRequest("One or more genres do not exist.");
        }

        var book = new Book
        {
            Title = dto.Title.Trim(),
            PublicationYear = dto.PublicationYear,
            AuthorId = dto.AuthorId,
            Genres = genres
        };

        await _bookRepository.AddAsync(book, cancellationToken);

        // Fetch author to return full DTO
        book.Author = await _authorRepository.GetByIdAsync(dto.AuthorId, cancellationToken);

        var result = MapToDto(book);
        return CreatedAtAction(nameof(GetById), new { id = book.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookUpsertDto dto, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);

        if (book is null)
        {
            return NotFound();
        }

        var authorExists = await _authorRepository.ExistsAsync(dto.AuthorId, cancellationToken);
        if (!authorExists)
        {
            return BadRequest("Author does not exist.");
        }

        var genres = await _genreRepository.GetByIdsAsync(dto.GenreIds, cancellationToken);

        if (genres.Count != dto.GenreIds.Count)
        {
            return BadRequest("One or more genres do not exist.");
        }

        book.Title = dto.Title.Trim();
        book.PublicationYear = dto.PublicationYear;
        book.AuthorId = dto.AuthorId;
        book.Genres = genres;

        await _bookRepository.UpdateAsync(book, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book is null)
        {
            return NotFound();
        }

        await _bookRepository.DeleteAsync(id, cancellationToken);

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
