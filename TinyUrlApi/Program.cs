using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using TinyUrlApi.Data;
using TinyUrlApi.DTOs;
using TinyUrlApi.Helpers;
using TinyUrlApi.Models;
using TinyUrlApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite("Data Source = tinyurl.db"));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200", "https://proud-desert-0a1d6b900.7.azurestaticapps.net")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});
builder.Services.AddSingleton<BlobLoggerService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowAngular");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}
app.MapPost("/api/shortenUrl",
    async (CreateShortUrlRequest request, AppDbContext db, IConfiguration config, BlobLoggerService logger)=>
    {
        try
        {
            var masterSecret =
           config["AppSettings:MasterSecret"];

            var shortSecretCode =
                Guid.NewGuid().ToString().Substring(0, 6);

            var rawToken =
                $"{shortSecretCode}-{masterSecret}-{Guid.NewGuid()}";

            var secretToken =
                Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(rawToken));

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
                SecretToken = secretToken,
                CreatedAt = DateTime.UtcNow,
            };
            db.ShortUrls.Add(entity);
            await db.SaveChangesAsync();
            await logger.LogAsync(
                $"Short URL created: {entity.ShortCode}");
            return Results.Ok(entity);
        }
        catch (Exception ex)
        {
            await logger.LogAsync(
            $"ERROR: {ex.Message}");

            return Results.Problem();
        }
    });
app.MapGet("/api/urls", async (AppDbContext db) =>
{
    return await db.ShortUrls.Where(s => !s.IsPrivate).ToListAsync();
});
app.MapGet("/api/{code}", async (string code, AppDbContext db) =>
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
    AppDbContext db, BlobLoggerService logger) =>
{
    var url = await db.ShortUrls
        .FirstOrDefaultAsync(x => x.ShortCode == code);

    if (url == null)
    {
        await logger.LogAsync($"Invalid shortcode access: {code}");
        return Results.NotFound();
    }

    // Increment clicks
    url.Clicks++;
    await logger.LogAsync($"Redirected shortcode: {code}");
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
app.MapDelete("/api/delete/{id}", async (int id, string token, AppDbContext db, BlobLoggerService logger) =>
{
    var entity = await db.ShortUrls.FindAsync(id);
    if (entity == null)
    {
        await logger.LogAsync($"Invalid shortcode access: {entity.ShortCode}");
        return Results.NotFound();
    }
    if (entity.SecretToken != token)
    {
        await logger.LogAsync($"Unauthorized user access to delete {entity.ShortCode}: {entity.Id}");
        return Results.Unauthorized();
    }
    db.ShortUrls.Remove(entity);
    await db.SaveChangesAsync();
    await logger.LogAsync($"Deleted URL id: {id}");
    return Results.Ok();
});

app.MapGet("/api/search", async (string query, AppDbContext db) =>
{ 
    var results = await db.ShortUrls.Where(s => s.ShortCode.Contains(query) || s.OriginalUrl.Contains(query)).ToListAsync();
    return Results.Ok(results);
});

//app.UseHttpsRedirection();
app.MapGet("/version", () => "Deployment Version 2");

app.UseAuthorization();

app.MapControllers();

app.Run();
