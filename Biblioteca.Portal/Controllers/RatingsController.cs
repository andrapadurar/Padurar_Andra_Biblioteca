
using BibliotecaGrpc;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;

namespace Biblioteca.Portal.Controllers;

public class RatingsController(
    ReaderRatings.ReaderRatingsClient client) : Controller
{
    public async Task<IActionResult> Index(int readerId = 1068)
    {
        var ratings = new List<RatingItem>();

        try
        {
            using var call = client.ListRatings(
                new ReaderRequest { ReaderId = readerId });

            await foreach (var rating in call.ResponseStream.ReadAllAsync())
            {
                ratings.Add(rating);
            }
        }
        catch (RpcException ex)
        {
            ViewBag.Error = $"{ex.StatusCode}: {ex.Status.Detail}";
        }

        ViewBag.ReaderId = readerId;
        return View(ratings);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var rating = await client.GetRatingAsync(
                new RatingId { Id = id });

            return View(rating);
        }
        catch (RpcException ex)
        {
            TempData["Error"] = ex.Status.Detail;
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Create(int readerId = 1068)
    {
        ViewBag.ReaderId = readerId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NewRating model)
    {
        try
        {
            await client.AddRatingAsync(model);
            return RedirectToAction(nameof(Index),
                new { readerId = model.ReaderId });
        }
        catch (RpcException ex)
        {
            ViewBag.Error = ex.Status.Detail;
            ViewBag.ReaderId = model.ReaderId;
            return View(model);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var rating = await client.GetRatingAsync(
                new RatingId { Id = id });

            return View(rating);
        }
        catch (RpcException ex)
        {
            TempData["Error"] = ex.Status.Detail;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, int stars)
    {
        try
        {
            await client.UpdateRatingAsync(
                new UpdateRatingRequest
                {
                    Id = id,
                    Stars = stars
                });

            return RedirectToAction(nameof(Index));
        }
        catch (RpcException ex)
        {
            TempData["Error"] = ex.Status.Detail;
            return RedirectToAction(nameof(Edit), new { id });
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var rating = await client.GetRatingAsync(
                new RatingId { Id = id });

            return View(rating);
        }
        catch (RpcException ex)
        {
            TempData["Error"] = ex.Status.Detail;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await client.DeleteRatingAsync(
                new RatingId { Id = id });

            return RedirectToAction(nameof(Index));
        }
        catch (RpcException ex)
        {
            TempData["Error"] = ex.Status.Detail;
            return RedirectToAction(nameof(Index));
        }
    }
}
