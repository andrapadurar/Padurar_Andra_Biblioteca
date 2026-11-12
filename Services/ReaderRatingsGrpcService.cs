
using BibliotecaGrpc;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Intalnirea1.Data;
using Intalnirea1.Models;
using Microsoft.EntityFrameworkCore;

public class ReaderRatingsGrpcService(LibraryContext db)
    : ReaderRatings.ReaderRatingsBase
{
    public override async Task ListRatings(
        ReaderRequest request,
        IServerStreamWriter<RatingItem> responseStream,
        ServerCallContext context)
    {
        if (request.ReaderId <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "reader_id trebuie sa fie un numar pozitiv."));

        var ratings = await db.Rating.AsNoTracking()
            .Where(r => r.ReaderId == request.ReaderId)
            .Join(db.Book,
                r => r.BookID,
                b => b.ID,
                (r, b) => new { Rating = r, Title = b.Title })
            .ToListAsync(context.CancellationToken);

        foreach (var item in ratings)
        {
            await responseStream.WriteAsync(
                ToMessage(item.Rating, item.Title));
        }
    }

    public override async Task<RatingItem> GetRating(
        RatingId request, ServerCallContext context)
    {
        var rating = await db.Rating.AsNoTracking()
            .FirstOrDefaultAsync(r => r.ID == request.Id,
                context.CancellationToken);

        if (rating == null)
            throw NotFound("Evaluarea nu exista.");

        var book = await db.Book.AsNoTracking()
            .FirstOrDefaultAsync(b => b.ID == rating.BookID,
                context.CancellationToken);

        if (book == null)
            throw NotFound("Cartea nu exista.");

        return ToMessage(rating, book.Title);
    }

    public override async Task<RatingItem> AddRating(
        NewRating request, ServerCallContext context)
    {
        ValidateReader(request.ReaderId);
        ValidateStars(request.Stars);

        var book = await db.Book.AsNoTracking()
            .FirstOrDefaultAsync(b => b.ID == request.BookId,
                context.CancellationToken);

        if (book == null)
            throw NotFound("Cartea nu exista.");

        var rating = new Rating
        {
            ReaderId = request.ReaderId,
            BookID = request.BookId,
            Stars = request.Stars,
            CreatedAt = DateTime.UtcNow
        };

        db.Rating.Add(rating);
        await db.SaveChangesAsync(context.CancellationToken);

        return ToMessage(rating, book.Title);
    }

    public override async Task<RatingItem> UpdateRating(
        UpdateRatingRequest request, ServerCallContext context)
    {
        ValidateStars(request.Stars);

        var rating = await db.Rating
            .FirstOrDefaultAsync(r => r.ID == request.Id,
                context.CancellationToken);

        if (rating == null)
            throw NotFound("Evaluarea nu exista.");

        var book = await db.Book.AsNoTracking()
            .FirstOrDefaultAsync(b => b.ID == rating.BookID,
                context.CancellationToken);

        if (book == null)
            throw NotFound("Cartea nu exista.");

        rating.Stars = request.Stars;
        await db.SaveChangesAsync(context.CancellationToken);

        return ToMessage(rating, book.Title);
    }

    public override async Task<Empty> DeleteRating(
        RatingId request, ServerCallContext context)
    {
        var rating = await db.Rating
            .FirstOrDefaultAsync(r => r.ID == request.Id,
                context.CancellationToken);

        if (rating == null)
            throw NotFound("Evaluarea nu exista.");

        db.Rating.Remove(rating);
        await db.SaveChangesAsync(context.CancellationToken);

        return new Empty();
    }

    private static void ValidateReader(int readerId)
    {
        if (readerId <= 0)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "reader_id trebuie sa fie un numar pozitiv."));
    }

    private static void ValidateStars(int stars)
    {
        if (stars < 1 || stars > 5)
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Nota trebuie sa fie intre 1 si 5."));
    }

    private static RpcException NotFound(string message) =>
        new(new Status(StatusCode.NotFound, message));

    private static RatingItem ToMessage(Rating rating, string title)
    {
        var utc = DateTime.SpecifyKind(
            rating.CreatedAt, DateTimeKind.Utc);

        return new RatingItem
        {
            Id = rating.ID,
            BookId = rating.BookID,
            Title = title,
            Stars = rating.Stars,
            CreatedAt = Timestamp.FromDateTime(utc)
        };
    }
}
