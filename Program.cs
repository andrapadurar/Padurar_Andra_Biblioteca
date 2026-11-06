
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

// MVC
builder.Services.AddControllersWithViews();

// Modelul de recomandare existent
builder.Services
    .AddPredictionEnginePool<BookRating, BookRatingPrediction>()
    .FromFile(
        modelName: "BookRecommender",
        filePath: "MLModels/BookRecommender.zip",
        watchForChanges: true);

// Laboratorul 5 - A2
builder.Services.AddScoped<RecommenderTrainer>();

// Ollama
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

// OpenAPI si Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// API-ul bibliotecii
app.MapLibraryApi();

// Endpoint AI pentru prezentarea unei carti
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
            "Esti bibliotecar. Raspunde in romana, in cel mult 2 propozitii."),
        new(
            ChatRole.User,
            $"Recomanda „{b.Title}” de {b.Author?.FullName} " +
            $"({b.Genre?.Name}). ")
        ];

        ChatResponse r = await ai.GetResponseAsync(messages);

        return Results.Ok(new
        {
            b.Title,
            Pitch = r.Text
        });
    });

// Endpoint pentru intrebari AI
app.MapGet("/api/v2/ask",
    async (string q, IChatClient ai) =>
        (await ai.GetResponseAsync(q)).Text);

// Migrari si date initiale
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<LibraryContext>();

    db.Database.Migrate();
    LibrarySeed.Seed(db);
}

// HTTP pipeline
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
