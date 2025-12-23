using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ITTicketSystem.Models;
using ITTicketSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ITTicketSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Debug action: check departments count and list a few
        public async Task<IActionResult> DeptCheck()
        {
            var departments = await _context.Departments
                .OrderBy(d => d.Id)
                .Take(10)
                .ToListAsync();

            var lines = new List<string>
            {
                $"Departments count: {await _context.Departments.CountAsync()}"
            };

            foreach (var d in departments)
            {
                lines.Add($"{d.Id} - {d.Name} (ParentId = {d.ParentDepartmentId?.ToString() ?? "null"})");
            }

            return Content(string.Join(Environment.NewLine, lines));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
