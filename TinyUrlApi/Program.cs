using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using System;
using TinyUrlApi.Data;
using TinyUrlApi.DTOs;
using TinyUrlApi.Helpers;
using TinyUrlApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AppDBContext>(options => options.UseSqlite("Data Source = tinyurl.db"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAngular");

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
app.MapGet("/api/{code}", async (string code, AppDBContext db) =>
{
var url = await db.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == code);
    if (url == null)
    {
        return Results.NotFound();
    }
        url.Clicks++;
        await db.SaveChangesAsync();
        return Results.Ok(url);
    
});
app.MapGet("/{code}", async (
    string code,
    AppDBContext db) =>
{
    var url = await db.ShortUrls
        .FirstOrDefaultAsync(x => x.ShortCode == code);

    if (url == null)
        return Results.NotFound();

    // Increment clicks
    url.Clicks++;

    await db.SaveChangesAsync();

    var originalUrl = url.OriginalUrl;

    // Add https if missing
    if (!originalUrl.StartsWith("http://") &&
        !originalUrl.StartsWith("https://"))
    {
        originalUrl = "https://" + originalUrl;
    }

    // Redirect
    return Results.Redirect(originalUrl);
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
    var results = await db.ShortUrls.Where(s => s.ShortCode.Contains(query) || s.OriginalUrl.Contains(query)).ToListAsync();
    return Results.Ok(results);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
