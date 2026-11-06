
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Intalnirea1.Data;
using Intalnirea1.Models;
using Intalnirea1.Services;

namespace Intalnirea1.Controllers
{
    public class AdminController(
        LibraryContext db,
        RecommenderTrainer trainer) : Controller
    {
        // GET: Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.Ratings = await db.Rating.CountAsync();
            return View();
        }

        // A1 - Import evaluari din CSV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ImportRatings(IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                TempData["Msg"] = "Alegeti un fisier CSV.";
                return RedirectToAction(nameof(Index));
            }

            var ratings = new List<Rating>();

            using var reader =
                new StreamReader(file.OpenReadStream());

            await reader.ReadLineAsync();

            string? line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                var p = line.Split(',');

                if (p.Length < 3 ||
                    !int.TryParse(p[0], out var u) ||
                    !int.TryParse(p[1], out var b) ||
                    !int.TryParse(p[2], out var s))
                {
                    continue;
                }

                if (u <= 0 || b <= 0 || s < 1 || s > 5)
                    continue;

                ratings.Add(new Rating
                {
                    ReaderId = u,
                    BookID = b,
                    Stars = s,
                    CreatedAt = DateTime.UtcNow
                });
            }

            db.Rating.AddRange(ratings);
            await db.SaveChangesAsync();

            TempData["Msg"] =
                $"Au fost importate {ratings.Count} evaluari.";

            return RedirectToAction(nameof(Index));
        }

        // A2 - Reantrenare din aplicatie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Retrain()
        {
            var (rmse, count) = await trainer.TrainAsync();

            TempData["Msg"] =
                $"Model reantrenat pe {count} evaluari. " +
                $"RMSE = {rmse:0.00}";

            return RedirectToAction(nameof(Index));
        }
    }
}
