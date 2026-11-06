
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Intalnirea1.Data;
using Intalnirea1.Models;

namespace Intalnirea1.Controllers
{
    public class OrdersController : Controller
    {
        private readonly LibraryContext _context;

        public OrdersController(LibraryContext context)
        {
            _context = context;
        }

        // GET: Orders
        // S1 - Lista comenzilor, sortare si filtrare dupa cititor
        public async Task<IActionResult> Index(int? customerId)
        {
            var customers = await _context.Customer
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Customers = new SelectList(
                customers, "CustomerID", "Name", customerId);

            ViewBag.SelectedCustomer = customerId;

            var orders = _context.Order
                .Include(o => o.Customer)
                .Include(o => o.Book)
                .AsNoTracking()
                .AsQueryable();

            if (customerId.HasValue)
            {
                orders = orders.Where(o =>
                    o.CustomerID == customerId.Value);
            }

            orders = orders.OrderByDescending(o => o.OrderDate)
                           .ThenByDescending(o => o.OrderID);

            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            LoadDropdowns();

            return View(new Order
            {
                OrderDate = DateTime.Today
            });
        }

        // POST: Orders/Create
        // Protectie la overposting folosind Bind
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CustomerID,BookID,OrderDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Order.Add(order);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(order.CustomerID, order.BookID);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Order.FindAsync(id);

            if (order == null)
                return NotFound();

            LoadDropdowns(order.CustomerID, order.BookID);

            return View(order);
        }

        // POST: Orders/Edit/5
        // Protectie la overposting folosind Bind
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("OrderID,CustomerID,BookID,OrderDate")] Order order)
        {
            if (id != order.OrderID)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingOrder =
                        await _context.Order.FindAsync(id);

                    if (existingOrder == null)
                        return NotFound();

                    existingOrder.CustomerID = order.CustomerID;
                    existingOrder.BookID = order.BookID;
                    existingOrder.OrderDate = order.OrderDate;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderID))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(order.CustomerID, order.BookID);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.Book)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);

            if (order != null)
            {
                _context.Order.Remove(order);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Dropdown-uri pentru cititori si carti
        private void LoadDropdowns(
            int? selectedCustomer = null,
            int? selectedBook = null)
        {
            ViewData["CustomerID"] = new SelectList(
                _context.Customer.OrderBy(c => c.Name),
                "CustomerID",
                "Name",
                selectedCustomer);

            ViewData["BookID"] = new SelectList(
                _context.Book.OrderBy(b => b.Title),
                "ID",
                "Title",
                selectedBook);
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(o => o.OrderID == id);
        }
    }
}
