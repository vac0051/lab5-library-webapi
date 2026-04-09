using Library.Api;
using Library.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var databaseProvider = (builder.Configuration["DatabaseProvider"] ?? "Sqlite").Trim();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<LibraryContext>(options =>
{
    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnection"));
        return;
    }

    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LibraryContext>();

    if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        await context.Database.EnsureCreatedAsync();
    }
    else
    {
        await context.Database.MigrateAsync();
    }

    await DbSeeder.SeedAsync(context, CancellationToken.None);
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
