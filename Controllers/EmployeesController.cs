using ITTicketSystem.Data;
using ITTicketSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace ITTicketSystem.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string RoleSessionKey = "CurrentRole";

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private UserRole GetCurrentRole()
        {
            var roleString = HttpContext.Session.GetString(RoleSessionKey);
            if (!string.IsNullOrEmpty(roleString) &&
                Enum.TryParse<UserRole>(roleString, out var role))
            {
                return role;
            }

            return UserRole.User;
        }

        private bool IsManager()
        {
            return GetCurrentRole() == UserRole.Manager;
        }

        // GET: /Employees
        public async Task<IActionResult> Index()
        {
            if (!IsManager())
            {
                return Forbid();
            }

            var employees = await _context.Employees
                .OrderBy(e => e.DepartmentName)
                .ThenBy(e => e.Name)
                .ToListAsync();

            return View(employees);
        }

        // GET: /Employees/Create
        public IActionResult Create()
        {
            if (!IsManager())
            {
                return Forbid();
            }

            return View(new Employee());
        }

        // POST: /Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Employee employee)
        {
            if (!IsManager())
            {
                return Forbid();
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(employee.Name))
            {
                ModelState.AddModelError(string.Empty, "Name is required.");
                return View(employee);
            }

            employee.IsActive = true;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: /Employees/ToggleArchive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleArchive(int id)
        {
            if (!IsManager())
            {
                return Forbid();
            }

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            employee.IsActive = !employee.IsActive;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
