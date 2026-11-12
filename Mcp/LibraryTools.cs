
using System;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using Intalnirea1.Data;

// Modelele pentru rezultatele instrumentelor MCP
public record BookInfo(
    int Id,
    string Title,
    string? Author,
    string? Genre,
    int Year,
    decimal Price);

public record BookDetails(
    int Id,
    string Title,
    string? Author,
    string? Genre,
    int Year,
    decimal Price,
    string? Description,
    int Orders);

// Laboratorul 9 - S2, varianta B2
public record MostOrderedBook(
    int BookId,
    string Title,
    int OrderCount);

[McpServerToolType]
public class LibraryTools
{
    // Cautarea cartilor dupa titlu sau autor
    [McpServerTool(
        Name = "search_books",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Cauta carti in catalog dupa titlu sau autor. " +
                 "Returneaza cel mult 10 carti.")]
    public static async Task<List<BookInfo>> SearchBooks(
        LibraryContext db,
        [Description("Cuvantul cautat, de exemplu Rebreanu sau Ion.")]
        string query,
        CancellationToken ct)
    {
        var q = query.Trim();

        return await db.Book.AsNoTracking()
            .Where(b => b.Title.Contains(q)
                || (b.Author != null &&
                    (b.Author.LastName.Contains(q) ||
                     b.Author.FirstName.Contains(q))))
            .OrderBy(b => b.Title)
            .Take(10)
            .Select(b => new BookInfo(
                b.ID,
                b.Title,
                b.Author == null
                    ? null
                    : b.Author.FirstName + " " + b.Author.LastName,
                b.Genre == null ? null : b.Genre.Name,
                b.Year,
                b.Price))
            .ToListAsync(ct);
    }

    // Detaliile unei carti
    [McpServerTool(
        Name = "get_book_details",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Returneaza detaliile unei carti dupa ID: " +
                 "autor, gen, an, pret, descriere si numar de comenzi.")]
    public static async Task<BookDetails?> GetBookDetails(
        LibraryContext db,
        [Description("ID-ul cartii din catalog.")]
        int bookId,
        CancellationToken ct)
    {
        return await db.Book.AsNoTracking()
            .Where(b => b.ID == bookId)
            .Select(b => new BookDetails(
                b.ID,
                b.Title,
                b.Author == null
                    ? null
                    : b.Author.FirstName + " " + b.Author.LastName,
                b.Genre == null ? null : b.Genre.Name,
                b.Year,
                b.Price,
                b.Description,
                b.Orders.Count()))
            .FirstOrDefaultAsync(ct);
    }

    // Recomandari folosind modelul ML.NET
    [McpServerTool(
        Name = "recommend_books",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Recomanda carti unui cititor folosind modelul ML.NET. " +
                 "Pentru cititorii noi poate folosi recomandari alternative.")]
    public static async Task<List<Recommendation>> RecommendBooks(
        RecommendationService recs,
        [Description("ID-ul cititorului.")]
        int readerId,
        [Description("Numarul de recomandari (1-10). Implicit 5.")]
        int top = 5,
        CancellationToken ct = default)
    {
        return await recs.RecommendAsync(
            readerId,
            Math.Clamp(top, 1, 10),
            ct);
    }

    // Instrument din laboratorul anterior
    [McpServerTool(
        Name = "top_books_by_price",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Returneaza cele mai scumpe sau cele mai ieftine carti.")]
    public static async Task<List<BookInfo>> TopBooksByPrice(
        LibraryContext db,
        [Description("true = cele mai scumpe, false = cele mai ieftine.")]
        bool mostExpensive,
        [Description("Numarul de carti (1-10). Implicit 3.")]
        int top = 3,
        CancellationToken ct = default)
    {
        var books = db.Book.AsNoTracking();

        books = mostExpensive
            ? books.OrderByDescending(b => b.Price)
            : books.OrderBy(b => b.Price);

        return await books
            .Take(Math.Clamp(top, 1, 10))
            .Select(b => new BookInfo(
                b.ID,
                b.Title,
                b.Author == null
                    ? null
                    : b.Author.FirstName + " " + b.Author.LastName,
                b.Genre == null ? null : b.Genre.Name,
                b.Year,
                b.Price))
            .ToListAsync(ct);
    }

    // Cautarea cartilor dupa gen literar
    [McpServerTool(
        Name = "books_by_genre",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Cauta carti din biblioteca dupa genul literar.")]
    public static async Task<List<BookInfo>> BooksByGenre(
        LibraryContext db,
        [Description("Genul literar cautat, de exemplu Roman sau Fantasy.")]
        string genre,
        CancellationToken ct = default)
    {
        return await db.Book.AsNoTracking()
            .Where(b => b.Genre != null &&
                        b.Genre.Name.Contains(genre))
            .Select(b => new BookInfo(
                b.ID,
                b.Title,
                b.Author == null
                    ? null
                    : b.Author.FirstName + " " + b.Author.LastName,
                b.Genre == null ? null : b.Genre.Name,
                b.Year,
                b.Price))
            .ToListAsync(ct);
    }

    // Laboratorul 9 - S2, varianta B2
    // Cele mai comandate carti
    [McpServerTool(
        Name = "most_ordered_books",
        ReadOnly = true,
        Destructive = false,
        OpenWorld = false)]
    [Description("Returneaza cele mai comandate carti din biblioteca, " +
                 "ordonate descrescator dupa numarul comenzilor.")]
    public static async Task<List<MostOrderedBook>> MostOrderedBooks(
        LibraryContext db,
        [Description("Numarul de carti returnate (1-10). Implicit 5.")]
        int top = 5,
        CancellationToken ct = default)
    {
        // Validarea parametrului top
        if (top < 1 || top > 10)
        {
            throw new ArgumentException(
                "Parametrul top trebuie sa fie intre 1 si 10.",
                nameof(top));
        }

        return await db.Book.AsNoTracking()
            .OrderByDescending(b => b.Orders.Count())
            .ThenBy(b => b.Title)
            .Take(top)
            .Select(b => new MostOrderedBook(
                b.ID,
                b.Title,
                b.Orders.Count()))
            .ToListAsync(ct);
    }
}
