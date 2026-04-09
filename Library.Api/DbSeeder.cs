using Library.Data;
using Library.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Api;

internal static class DbSeeder
{
    public static async Task SeedAsync(LibraryContext context, CancellationToken cancellationToken)
    {
        if (await context.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        var sciFi = new Genre { Name = "Sci-Fi" };
        var fantasy = new Genre { Name = "Fantasy" };
        var drama = new Genre { Name = "Drama" };

        var asimov = new Author { Name = "Isaac Asimov" };
        var tolkien = new Author { Name = "J.R.R. Tolkien" };
        var orwell = new Author { Name = "George Orwell" };

        context.Books.AddRange(
            new Book
            {
                Title = "Foundation",
                PublicationYear = 1951,
                Author = asimov,
                Genres = new List<Genre> { sciFi }
            },
            new Book
            {
                Title = "The Hobbit",
                PublicationYear = 1937,
                Author = tolkien,
                Genres = new List<Genre> { fantasy }
            },
            new Book
            {
                Title = "Animal Farm",
                PublicationYear = 1945,
                Author = orwell,
                Genres = new List<Genre> { drama }
            },
            new Book
            {
                Title = "The Lord of the Rings",
                PublicationYear = 1954,
                Author = tolkien,
                Genres = new List<Genre> { fantasy, drama }
            });

        await context.SaveChangesAsync(cancellationToken);
    }
}
