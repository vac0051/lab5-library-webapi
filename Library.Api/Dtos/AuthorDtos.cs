using System.ComponentModel.DataAnnotations;

namespace Library.Api.Dtos;

public sealed class AuthorUpsertDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;
}

public sealed class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<int> BookIds { get; set; } = [];
}
