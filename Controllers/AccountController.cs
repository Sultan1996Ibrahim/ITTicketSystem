using ITTicketSystem.Data;
using ITTicketSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ITTicketSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string RoleSessionKey = "CurrentRole";
        private const string UserNameSessionKey = "CurrentUserName";
        private const string UserIdSessionKey = "CurrentUserId";

        // ✅ Manager stores multiple departments
        private const string ManagerDeptIdsSessionKey = "ManagerDeptIds";

        // ✅ User stores single department
        private const string DepartmentIdSessionKey = "CurrentDepartmentId";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var roleString = HttpContext.Session.GetString(RoleSessionKey);
            if (!string.IsNullOrEmpty(roleString) &&
                Enum.TryParse<UserRole>(roleString, out var role))
            {
                return RedirectAfterLogin(role);
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == model.UserName && u.IsActive);

            if (user == null || !PasswordHelper.Verify(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            // ✅ Basic sessions
            HttpContext.Session.SetString(RoleSessionKey, user.Role.ToString());
            HttpContext.Session.SetString(UserNameSessionKey, user.UserName);
            HttpContext.Session.SetInt32(UserIdSessionKey, user.Id);

            // ✅ clear old sessions
            HttpContext.Session.Remove(DepartmentIdSessionKey);
            HttpContext.Session.Remove(ManagerDeptIdsSessionKey);

            // ✅ User: store single DepartmentId
            if (user.Role == UserRole.User && user.DepartmentId.HasValue)
            {
                HttpContext.Session.SetInt32(DepartmentIdSessionKey, user.DepartmentId.Value);
            }

            // ✅ Manager: store multiple dept ids in session
            if (user.Role == UserRole.Manager)
            {
                var managedDeptIds = await _context.ManagerDepartments
                    .Where(x => x.ManagerUserId == user.Id)
                    .Select(x => x.DepartmentId)
                    .ToListAsync();

                HttpContext.Session.SetString(
                    ManagerDeptIdsSessionKey,
                    string.Join(",", managedDeptIds)
                );
            }

            return RedirectAfterLogin(user.Role);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        private IActionResult RedirectAfterLogin(UserRole role)
        {
            if (role == UserRole.Manager)
                return RedirectToAction("ManagerDashboard", "Tickets");

            if (role == UserRole.Admin)
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Tickets");
        }
    }
}
