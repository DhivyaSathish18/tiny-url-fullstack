using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Data;
using TinyUrlApi.DTOs;
using TinyUrlApi.Helpers;
using TinyUrlApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlite("Data Source = tinyurl.db"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapPost("/api/shortenUrl",
    async (CreateShortUrlRequest request, AppDBContext db) =>
    {
        string code;
        do
        {
            code = ShortCodeGenerator.Generate();
        }
        while (await db.ShortUrls.AnyAsync(s => s.ShortCode == code));
        var entity = new ShortUrl
        {
            OriginalUrl = request.OriginalUrl,
            IsPrivate = request.IsPrivate,
            ShortCode = code,
            CreatedAt = DateTime.UtcNow
        };
        db.ShortUrls.Add(entity);
        await db.SaveChangesAsync();
        return Results.Ok(entity);
    });
app.MapGet("/api/urls", async (AppDBContext db) =>
{
    return await db.ShortUrls.Where(s => !s.IsPrivate).ToListAsync();
});
app.MapGet("/{code}", async (string code, AppDBContext db) =>
{
var url = await db.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == code);
    if (url == null)
    {
        return Results.NotFound();
    }
        url.Clicks++;
        await db.SaveChangesAsync();
        return Results.Redirect(url.OriginalUrl);
    
});
app.MapDelete("/api/{id}", async (int id, AppDBContext db) =>
{
    var entity = await db.ShortUrls.FindAsync(id);
    if (entity == null)
    {
        return Results.NotFound();
    }
    db.ShortUrls.Remove(entity);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.MapGet("/api/search", async (string query, AppDBContext db) =>
{
    return await db.ShortUrls.Where(s => s.ShortCode.Contains(query)).ToListAsync();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
