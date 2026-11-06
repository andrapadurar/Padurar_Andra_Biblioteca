
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Intalnirea1.Data;
using Intalnirea1.Models;
using Intalnirea1.Api;
using Intalnirea1.Services;
using Microsoft.Extensions.ML;
using Scalar.AspNetCore;

using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("LibraryContext")
        ?? throw new InvalidOperationException(
            "Connection string 'LibraryContext' not found.")));

builder.Services.AddControllersWithViews();

builder.Services
    .AddPredictionEnginePool<BookRating, BookRatingPrediction>()
    .FromFile(
        modelName: "BookRecommender",
        filePath: "MLModels/BookRecommender.zip",
        watchForChanges: true);

// A2 - Serviciul de reantrenare
builder.Services.AddScoped<RecommenderTrainer>();

var endpoint = new Uri("http://localhost:11434/v1/");

IChatClient chat = new OpenAIClient(
    new ApiKeyCredential("ollama"),
    new OpenAIClientOptions { Endpoint = endpoint })
    .GetChatClient("llama3.2")
    .AsIChatClient();

builder.Services.AddSingleton<IChatClient>(chat);

builder.Services.AddValidation();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapLibraryApi();

app.MapPost("/api/v2/books/{id:int}/pitch",
    async (int id, LibraryContext db, IChatClient ai) =>
    {
        var b = await db.Book
            .Include(x => x.Author)
            .Include(x => x.Genre)
            .FirstOrDefaultAsync(x => x.ID == id);

        if (b is null)
            return Results.NotFound();

        List<ChatMessage> messages =
        [
            new(
            ChatRole.System,
            "Ești bibliotecar. Răspunde în română, în cel mult 2 propoziții."),
        new(
            ChatRole.User,
            $"Recomandă „{b.Title}” de {b.Author?.FullName} " +
            $"({b.Genre?.Name}). ")
        ];

        ChatResponse r = await ai.GetResponseAsync(messages);

        return Results.Ok(new
        {
            b.Title,
            Pitch = r.Text
        });
    });

app.MapGet("/api/v2/ask",
    async (string q, IChatClient ai) =>
        (await ai.GetResponseAsync(q)).Text);

// A3 - API de recomandari cu cold start
app.MapGet("/api/v2/readers/{id:int}/recommendations",
    async (
        int id,
        int? top,
        LibraryContext db,
        PredictionEnginePool<BookRating, BookRatingPrediction> pool) =>
    {
        var n = Math.Clamp(top ?? 5, 1, 20);

        var rated = await db.Rating
            .Where(r => r.ReaderId == id)
            .Select(r => r.BookID)
            .ToListAsync();

        var candidates = await db.Book
            .AsNoTracking()
            .Where(b => !rated.Contains(b.ID))
            .Select(b => new { b.ID, b.Title })
            .ToListAsync();

        var recs = candidates
            .Select(b => new RecommendationDto(
                b.ID,
                b.Title,
                pool.Predict(
                    "BookRecommender",
                    new BookRating
                    {
                        UserId = id,
                        BookId = b.ID
                    }).Score,
                false))
            .Where(r => !float.IsNaN(r.Score))
            .OrderByDescending(r => r.Score)
            .Take(n)
            .ToList();

        if (recs.Count == 0)
        {
            // Prag A3: minimum 12 evaluari pentru fiecare carte
            var popular = await db.Rating
                .GroupBy(r => r.BookID)
                .Where(g => g.Count() >= 12)
                .Select(g => new
                {
                    BookID = g.Key,
                    Avg = g.Average(r => r.Stars)
                })
                .OrderByDescending(x => x.Avg)
                .Take(n)
                .ToListAsync();

            var ids = popular.Select(p => p.BookID).ToList();

            var titles = await db.Book
                .Where(b => ids.Contains(b.ID))
                .ToDictionaryAsync(b => b.ID, b => b.Title);

            recs = popular
                .Select(p => new RecommendationDto(
                    p.BookID,
                    titles[p.BookID],
                    (float)p.Avg,
                    true))
                .ToList();
        }

        return TypedResults.Ok(recs);
    })
.WithTags("Recomandări");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<LibraryContext>();

    db.Database.Migrate();
    LibrarySeed.Seed(db);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization("en-US");

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "start",
    pattern: "start",
    defaults: new
    {
        controller = "Home",
        action = "Index"
    });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
