using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public sealed class BookUpsertDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int PublicationYear { get; set; }

    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }

    [Required]
    [MinLength(1)]
    public List<int> GenreIds { get; set; } = [];
}

public sealed class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public int AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public IReadOnlyList<string> Genres { get; set; } = [];
}
