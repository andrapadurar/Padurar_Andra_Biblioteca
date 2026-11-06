
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Intalnirea1.Data;
using Intalnirea1.Models;

namespace Intalnirea1.Controllers
{
    public class AuthorsController : Controller
    {
        private readonly LibraryContext _context;

        public AuthorsController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Authors
        public async Task<IActionResult> Index()
        {
            return View(await _context.Author.ToListAsync());
        }

        // GET: Authors/Details/5
        // S2 - Cartile autorului si indicatorul B2
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ID == id);

            if (author == null)
                return NotFound();

            // Lista cartilor autorului
            var books = await _context.Book
                .AsNoTracking()
                .Where(b => b.AuthorID == id)
                .OrderBy(b => b.Title)
                .Select(b => new BookListItem(
                    b.ID,
                    b.Title,
                    b.Author != null
                        ? b.Author.FirstName + " " + b.Author.LastName
                        : "",
                    b.Genre != null ? b.Genre.Name : "",
                    b.Price,
                    b.Orders.Count()
                ))
                .ToListAsync();

            // Calcularea indicatorilor direct in SQL
            var statistics = await _context.Book
                .Where(b => b.AuthorID == id)
                .GroupBy(b => b.AuthorID)
                .Select(g => new
                {
                    TotalOrders = g.Sum(b => b.Orders.Count()),
                    FirstYear = g.Min(b => b.Year),
                    LastYear = g.Max(b => b.Year)
                })
                .FirstOrDefaultAsync();

            ViewBag.Books = books;
            ViewBag.TotalOrders = statistics?.TotalOrders ?? 0;
            ViewBag.FirstYear = statistics?.FirstYear;
            ViewBag.LastYear = statistics?.LastYear;

            return View(author);
        }

        // GET: Authors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("ID,FirstName,LastName")] Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: Authors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author.FindAsync(id);

            if (author == null)
                return NotFound();

            return View(author);
        }

        // POST: Authors/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ID,FirstName,LastName")] Author author)
        {
            if (id != author.ID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(author);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuthorExists(author.ID))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: Authors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var author = await _context.Author
                .FirstOrDefaultAsync(a => a.ID == id);

            if (author == null)
                return NotFound();

            return View(author);
        }

        // POST: Authors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Author.FindAsync(id);

            if (author != null)
            {
                _context.Author.Remove(author);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AuthorExists(int id)
        {
            return _context.Author.Any(a => a.ID == id);
        }
    }
}
