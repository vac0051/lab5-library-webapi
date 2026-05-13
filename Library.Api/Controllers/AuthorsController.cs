using Library.Api.Dtos;
using Library.Data;
using Library.Data.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthorsController : ControllerBase
{
    private readonly AuthorRepository _authorRepository;

    public AuthorsController(AuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AuthorDto>>> GetAll(CancellationToken cancellationToken)
    {
        var authors = await _authorRepository.GetAllAsync(cancellationToken);
        
        var dtos = authors.Select(author => new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            BookIds = [] // В рамках ADO.NET мы не подтягиваем книги сразу, чтобы избежать N+1, возвращаем пустой список
        }).ToList();

        return Ok(dtos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AuthorDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken);

        if (author is null)
        {
            return NotFound();
        }

        return Ok(new AuthorDto
        {
            Id = author.Id,
            Name = author.Name,
            BookIds = [] 
        });
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> Create(AuthorUpsertDto dto, CancellationToken cancellationToken)
    {
        var author = new Author
        {
            Name = dto.Name.Trim()
        };

        await _authorRepository.AddAsync(author, cancellationToken);

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
        var exists = await _authorRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        var author = new Author { Id = id, Name = dto.Name.Trim() };
        await _authorRepository.UpdateAsync(author, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var exists = await _authorRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            return NotFound();
        }

        await _authorRepository.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
