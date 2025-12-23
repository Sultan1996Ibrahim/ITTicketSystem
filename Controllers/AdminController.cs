using ITTicketSystem.Data;
using ITTicketSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITTicketSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string RoleSessionKey = "CurrentRole";

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            var roleString = HttpContext.Session.GetString(RoleSessionKey);
            return roleString == UserRole.Admin.ToString();
        }

        // Helper: load departments for views (dropdown + checklist)
        private async Task LoadDepartmentsAsync(int? selectedUserDeptId = null)
        {
            var departments = await _context.Departments
                .Where(d => d.ParentDepartmentId != null)
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.Departments = departments;
            ViewBag.UserDepartmentsSelectList = new SelectList(departments, "Id", "Name", selectedUserDeptId);
        }

        // ========================= Admin Dashboard (Tickets) =========================
        public async Task<IActionResult> Dashboard(
            int? departmentId,
            string? ticketNumber,
            string? title,
            string? department,
            string? fromDepartment,
            string? createdBy,
            string? assignedTo,
            string? status,
            string? createdAt,
            string? sort,
            string? dir)
        {
            if (!IsAdmin()) return Forbid();

            var departmentsList = await _context.Departments
                .Where(d => d.ParentDepartmentId != null)
                .OrderBy(d => d.Name)
                .ToListAsync();

            ViewBag.Departments = new SelectList(departmentsList, "Id", "Name", departmentId);

            var q = _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.FromDepartment)
                .Include(t => t.AssignedUser)
                .AsQueryable();

            if (departmentId.HasValue)
                q = q.Where(t => t.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(ticketNumber))
                q = q.Where(t => t.ReferenceNumber != null && t.ReferenceNumber.Contains(ticketNumber.Trim()));

            if (!string.IsNullOrWhiteSpace(title))
                q = q.Where(t => t.Title.Contains(title.Trim()));

            if (!string.IsNullOrWhiteSpace(department))
                q = q.Where(t => t.Department != null && t.Department.Name.Contains(department.Trim()));

            if (!string.IsNullOrWhiteSpace(fromDepartment))
                q = q.Where(t => t.FromDepartment != null && t.FromDepartment.Name.Contains(fromDepartment.Trim()));

            if (!string.IsNullOrWhiteSpace(createdBy))
                q = q.Where(t => t.CreatedBy != null && t.CreatedBy.Contains(createdBy.Trim()));

            if (!string.IsNullOrWhiteSpace(assignedTo))
                q = q.Where(t => t.AssignedUser != null && t.AssignedUser.UserName.Contains(assignedTo.Trim()));

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TicketStatus>(status, out var st))
                q = q.Where(t => t.Status == st);

            if (!string.IsNullOrWhiteSpace(createdAt) && DateTime.TryParse(createdAt, out var dt))
                q = q.Where(t => t.CreatedAt.Date == dt.Date);

            var isDesc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            var s = (sort ?? "").Trim().ToLowerInvariant();

            q = s switch
            {
                "ticketnumber"   => isDesc ? q.OrderByDescending(t => t.ReferenceNumber) : q.OrderBy(t => t.ReferenceNumber),
                "title"          => isDesc ? q.OrderByDescending(t => t.Title) : q.OrderBy(t => t.Title),
                "department"     => isDesc ? q.OrderByDescending(t => t.Department!.Name) : q.OrderBy(t => t.Department!.Name),
                "fromdepartment" => isDesc ? q.OrderByDescending(t => t.FromDepartment!.Name) : q.OrderBy(t => t.FromDepartment!.Name),
                "createdby"      => isDesc ? q.OrderByDescending(t => t.CreatedBy) : q.OrderBy(t => t.CreatedBy),
                "assignedto"     => isDesc ? q.OrderByDescending(t => t.AssignedUser!.UserName) : q.OrderBy(t => t.AssignedUser!.UserName),
                "status"         => isDesc ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status),
                "createdat"      => isDesc ? q.OrderByDescending(t => t.CreatedAt) : q.OrderBy(t => t.CreatedAt),
                _                => q.OrderByDescending(t => t.CreatedAt)
            };

            return View(await q.ToListAsync());
        }

        // ========================= Users List =========================
        public async Task<IActionResult> Index(string? search)
        {
            if (!IsAdmin()) return Forbid();

            var q = _context.AppUsers
                .Include(u => u.Department)
                .Include(u => u.ManagedDepartments)
                    .ThenInclude(md => md.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(u => u.UserName.Contains(s));
            }

            ViewBag.Search = search;

            var users = await q.OrderBy(u => u.UserName).ToListAsync();
            return View(users);
        }

        // ========================= Create User =========================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!IsAdmin()) return Forbid();

            await LoadDepartmentsAsync();
            ViewBag.AllDepartments = (List<Department>)ViewBag.Departments;

            return View(new AdminCreateUserViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminCreateUserViewModel model)
        {
            if (!IsAdmin()) return Forbid();

            if (model.Role == UserRole.Manager &&
                (model.ManagedDepartmentIds == null || !model.ManagedDepartmentIds.Any()))
            {
                ModelState.AddModelError("", "Manager must have at least one department.");
            }

            // ✅ User لازم يختار Department
            if (model.Role == UserRole.User && !model.DepartmentId.HasValue)
            {
                ModelState.AddModelError("DepartmentId", "Department is required for User.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync(model.DepartmentId);
                ViewBag.AllDepartments = (List<Department>)ViewBag.Departments;
                return View(model);
            }

            var exists = await _context.AppUsers.AnyAsync(u => u.UserName == model.UserName);
            if (exists)
            {
                ModelState.AddModelError("UserName", "Username already exists.");
                await LoadDepartmentsAsync(model.DepartmentId);
                ViewBag.AllDepartments = (List<Department>)ViewBag.Departments;
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.UserName,
                PasswordHash = PasswordHelper.Hash(model.Password),
                Role = model.Role,
                IsActive = model.IsActive,
                DepartmentId = model.Role == UserRole.User ? model.DepartmentId : null,
                CanManageDeptTickets = (model.Role == UserRole.User) && model.CanManageDeptTickets
            };

            _context.AppUsers.Add(user);
            await _context.SaveChangesAsync();

            if (user.Role == UserRole.Manager)
            {
                foreach (var depId in (model.ManagedDepartmentIds ?? new List<int>()).Distinct())
                {
                    _context.ManagerDepartments.Add(new ManagerDepartment
                    {
                        ManagerUserId = user.Id,
                        DepartmentId = depId
                    });
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ========================= Edit User =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return Forbid();

            var user = await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            await LoadDepartmentsAsync(user.DepartmentId);
            ViewBag.AllDepartments = (List<Department>)ViewBag.Departments;

            var managedIds = await _context.ManagerDepartments
                .Where(x => x.ManagerUserId == id)
                .Select(x => x.DepartmentId)
                .ToListAsync();

            var vm = new AppUserEditVM
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.Role,
                IsActive = user.IsActive,
                DepartmentId = user.DepartmentId,
                ManagedDepartmentIds = managedIds,
                CanManageDeptTickets = user.CanManageDeptTickets
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AppUserEditVM vm)
        {
            if (!IsAdmin()) return Forbid();

            if (vm.Role == UserRole.Manager &&
                (vm.ManagedDepartmentIds == null || !vm.ManagedDepartmentIds.Any()))
            {
                ModelState.AddModelError("", "Manager must have at least one department.");
            }

            // ✅ User لازم يختار Department (مهم جدًا لحل FromDepartment)
            if (vm.Role == UserRole.User && !vm.DepartmentId.HasValue)
            {
                ModelState.AddModelError("DepartmentId", "Department is required for User.");
            }

            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync(vm.DepartmentId);
                ViewBag.AllDepartments = (List<Department>)ViewBag.Departments;
                return View(vm);
            }

            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == vm.Id);
            if (user == null) return NotFound();

            user.Role = vm.Role;
            user.IsActive = vm.IsActive;

            user.DepartmentId = vm.Role == UserRole.User ? vm.DepartmentId : null;
            user.CanManageDeptTickets = (vm.Role == UserRole.User) && vm.CanManageDeptTickets;

            await _context.SaveChangesAsync();

            var oldLinks = await _context.ManagerDepartments
                .Where(x => x.ManagerUserId == user.Id)
                .ToListAsync();

            _context.ManagerDepartments.RemoveRange(oldLinks);

            if (user.Role == UserRole.Manager)
            {
                foreach (var depId in (vm.ManagedDepartmentIds ?? new List<int>()).Distinct())
                {
                    _context.ManagerDepartments.Add(new ManagerDepartment
                    {
                        ManagerUserId = user.Id,
                        DepartmentId = depId
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
