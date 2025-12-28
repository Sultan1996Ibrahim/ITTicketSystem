using ITTicketSystem.Data;
using ITTicketSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace ITTicketSystem.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string RoleSessionKey = "CurrentRole";
        private const string UserNameSessionKey = "CurrentUserName";
        private const string UserIdSessionKey = "CurrentUserId";

        private const string DepartmentIdSessionKey = "CurrentDepartmentId"; // User only
        private const string ManagerDeptIdsSessionKey = "ManagerDeptIds";     // Manager only

        public TicketsController(ApplicationDbContext context)
        {
            _context = context;
        }

        private IActionResult Forbidden() => StatusCode(StatusCodes.Status403Forbidden);

        private bool IsAjaxRequest()
        {
            var requestedWith = Request.Headers["X-Requested-With"].ToString();
            return string.Equals(requestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
        }

        private UserRole GetCurrentRole()
        {
            var roleString = HttpContext.Session.GetString(RoleSessionKey);
            if (!string.IsNullOrEmpty(roleString) && Enum.TryParse<UserRole>(roleString, out var role))
                return role;

            return UserRole.User;
        }

        private string GetCurrentUserName()
            => HttpContext.Session.GetString(UserNameSessionKey) ?? "Unknown";

        private int? GetCurrentUserId()
            => HttpContext.Session.GetInt32(UserIdSessionKey);

        private int? GetCurrentDepartmentId()
            => HttpContext.Session.GetInt32(DepartmentIdSessionKey);

        private List<int> GetManagerDeptIds()
        {
            var s = HttpContext.Session.GetString(ManagerDeptIdsSessionKey);
            if (string.IsNullOrWhiteSpace(s)) return new List<int>();

            return s.Split(',')
                    .Select(x => int.TryParse(x, out var v) ? v : (int?)null)
                    .Where(v => v.HasValue)
                    .Select(v => v!.Value)
                    .Distinct()
                    .ToList();
        }

        // ✅ Get sender department robustly (works for User; Manager might be null by design)
        private async Task<int?> GetSenderDepartmentIdAsync()
        {
            var deptId = GetCurrentDepartmentId();
            if (deptId.HasValue) return deptId.Value;

            var uid = GetCurrentUserId();
            if (!uid.HasValue) return null;

            return await _context.AppUsers
                .Where(u => u.Id == uid.Value)
                .Select(u => u.DepartmentId)
                .FirstOrDefaultAsync();
        }

        private IQueryable<Ticket> ApplyGridFiltersAndSort(
            IQueryable<Ticket> q,
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
            {
                var d = dt.Date;
                q = q.Where(t => t.CreatedAt.Date == d);
            }

            var isDesc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            var s = (sort ?? "").Trim().ToLowerInvariant();

            q = s switch
            {
                "ticketnumber" => isDesc ? q.OrderByDescending(t => t.ReferenceNumber) : q.OrderBy(t => t.ReferenceNumber),
                "title" => isDesc ? q.OrderByDescending(t => t.Title) : q.OrderBy(t => t.Title),
                "department" => isDesc ? q.OrderByDescending(t => t.Department!.Name) : q.OrderBy(t => t.Department!.Name),
                "fromdepartment" => isDesc ? q.OrderByDescending(t => t.FromDepartment!.Name) : q.OrderBy(t => t.FromDepartment!.Name),
                "createdby" => isDesc ? q.OrderByDescending(t => t.CreatedBy) : q.OrderBy(t => t.CreatedBy),
                "assignedto" => isDesc ? q.OrderByDescending(t => t.AssignedUser!.UserName) : q.OrderBy(t => t.AssignedUser!.UserName),
                "status" => isDesc ? q.OrderByDescending(t => t.Status) : q.OrderBy(t => t.Status),
                "createdat" => isDesc ? q.OrderByDescending(t => t.CreatedAt) : q.OrderBy(t => t.CreatedAt),
                _ => q.OrderByDescending(t => t.CreatedAt)
            };

            return q;
        }

        public async Task<IActionResult> Index(
            string? ticketNumber,
            string? title,
            string? department,
            string? fromDepartment,
            string? status,
            string? createdAt,
            string? sort,
            string? dir)
        {
            var role = GetCurrentRole();
            var userName = GetCurrentUserName();

            var q = _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.FromDepartment)
                .Include(t => t.AssignedUser)
                .Where(t => t.CreatedBy == userName);

            q = ApplyGridFiltersAndSort(
                q,
                ticketNumber,
                title,
                department,
                fromDepartment,
                createdBy: null,
                assignedTo: null,
                status,
                createdAt,
                sort,
                dir);

            ViewData["CurrentRole"] = role;
            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> UserDashboard(
            TicketStatus? filter,
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
            var role = GetCurrentRole();
            if (role != UserRole.User) return Forbidden();

            var userDeptId = GetCurrentDepartmentId();
            if (!userDeptId.HasValue) return Forbidden();

            var deptQuery = _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.FromDepartment)
                .Include(t => t.AssignedUser)
                .Where(t => t.DepartmentId == userDeptId.Value);

            var model = new UserDashboardViewModel
            {
                TotalTickets = await deptQuery.CountAsync(),
                NewCount = await deptQuery.CountAsync(t => t.Status == TicketStatus.New),
                InProgressCount = await deptQuery.CountAsync(t =>
                    t.Status == TicketStatus.AssignedToDepartment ||
                    t.Status == TicketStatus.InProgress),
                ClosedCount = await deptQuery.CountAsync(t => t.Status == TicketStatus.Closed),
                CurrentFilter = filter
            };

            IQueryable<Ticket> listQuery = deptQuery;

            if (filter.HasValue)
            {
                if (filter.Value == TicketStatus.InProgress)
                {
                    listQuery = listQuery.Where(t =>
                        t.Status == TicketStatus.AssignedToDepartment ||
                        t.Status == TicketStatus.InProgress);
                }
                else
                {
                    listQuery = listQuery.Where(t => t.Status == filter.Value);
                }
            }

            listQuery = ApplyGridFiltersAndSort(
                listQuery,
                ticketNumber,
                title,
                department,
                fromDepartment,
                createdBy,
                assignedTo,
                status,
                createdAt,
                sort,
                dir);

            model.Tickets = await listQuery.ToListAsync();

            ViewData["CurrentRole"] = role;

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                ViewBag.CanManage = await _context.AppUsers
                    .Where(u => u.Id == userId.Value)
                    .Select(u => u.CanManageDeptTickets)
                    .FirstOrDefaultAsync();
            }
            else
            {
                ViewBag.CanManage = false;
            }

            return View(model);
        }

        public async Task<IActionResult> MyAssigned(
            string? ticketNumber,
            string? title,
            string? department,
            string? fromDepartment,
            string? status,
            string? createdAt,
            string? sort,
            string? dir)
        {
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            if (!userId.HasValue) return Forbidden();

            var q = _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.FromDepartment)
                .Include(t => t.AssignedUser)
                .Where(t => t.AssignedUserId == userId.Value);

            q = ApplyGridFiltersAndSort(
                q,
                ticketNumber,
                title,
                department,
                fromDepartment,
                createdBy: null,
                assignedTo: null,
                status,
                createdAt,
                sort,
                dir);

            ViewData["CurrentRole"] = role;
            return View(await q.ToListAsync());
        }

        public async Task<IActionResult> ManagerDashboard(
            TicketStatus? filter,
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
            var role = GetCurrentRole();
            if (role != UserRole.Manager) return Forbidden();

            var managedDeptIds = GetManagerDeptIds();
            if (managedDeptIds.Count == 0) return Forbidden();

            var baseQuery = _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.FromDepartment)
                .Include(t => t.AssignedUser)
                .Where(t => managedDeptIds.Contains(t.DepartmentId));

            ViewBag.TotalTickets = await baseQuery.CountAsync();
            ViewBag.AwaitingApproval = await baseQuery.CountAsync(t => t.Status == TicketStatus.New);
            ViewBag.WorkInProgress = await baseQuery.CountAsync(t =>
                t.Status == TicketStatus.AssignedToDepartment ||
                t.Status == TicketStatus.InProgress);
            ViewBag.ClosedTickets = await baseQuery.CountAsync(t => t.Status == TicketStatus.Closed);
            ViewBag.CurrentFilter = filter;

            IQueryable<Ticket> listQuery = baseQuery;

            if (filter.HasValue)
            {
                switch (filter.Value)
                {
                    case TicketStatus.New:
                        listQuery = listQuery.Where(t => t.Status == TicketStatus.New);
                        break;

                    case TicketStatus.InProgress:
                        listQuery = listQuery.Where(t =>
                            t.Status == TicketStatus.AssignedToDepartment ||
                            t.Status == TicketStatus.InProgress);
                        break;

                    case TicketStatus.Closed:
                        listQuery = listQuery.Where(t => t.Status == TicketStatus.Closed);
                        break;
                }
            }

            listQuery = ApplyGridFiltersAndSort(
                listQuery,
                ticketNumber,
                title,
                department,
                fromDepartment,
                createdBy,
                assignedTo,
                status,
                createdAt,
                sort,
                dir);

            ViewData["CurrentRole"] = role;
            return View(await listQuery.ToListAsync());
        }

       public IActionResult Create()
{
    var role = GetCurrentRole();
    var userDeptId = GetCurrentDepartmentId();
    var managerDeptIds = GetManagerDeptIds();

    var departmentsQuery = _context.Departments
        .Where(d => d.ParentDepartmentId != null); 

    if (role == UserRole.User && userDeptId.HasValue)
        departmentsQuery = departmentsQuery.Where(d => d.Id != userDeptId.Value);

    if (role == UserRole.Manager && managerDeptIds.Any())
        departmentsQuery = departmentsQuery.Where(d => !managerDeptIds.Contains(d.Id));

    var departments = departmentsQuery.OrderBy(d => d.Name).ToList();

    ViewData["DepartmentId"] = new SelectList(departments, "Id", "Name");
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("Title,Description,DepartmentId")] Ticket ticket, List<IFormFile> files)
{
    var role = GetCurrentRole();
    var userDeptId = GetCurrentDepartmentId();
    var managerDeptIds = GetManagerDeptIds();

    var departmentExists = await _context.Departments
        .AnyAsync(d => d.Id == ticket.DepartmentId && d.ParentDepartmentId != null);

    if (!departmentExists)
        ModelState.AddModelError("DepartmentId", "Selected department does not exist.");

    
    if (role == UserRole.User && userDeptId.HasValue)
    {
        var userDept = await _context.Departments
            .Where(d => d.Id == userDeptId.Value)
            .Select(d => new { d.Id, d.ParentDepartmentId })
            .FirstOrDefaultAsync();

        if (userDept != null)
        {
            
            var userRootId = userDept.ParentDepartmentId ?? userDept.Id;

           
            var targetRootId = await _context.Departments
                .Where(d => d.Id == ticket.DepartmentId)
                .Select(d => d.ParentDepartmentId)
                .FirstOrDefaultAsync();

            if (targetRootId.HasValue && targetRootId.Value == userRootId)
                ModelState.AddModelError("DepartmentId", "You cannot create a ticket for your own department.");
        }
    }

    
    if (role == UserRole.Manager && managerDeptIds.Any() && managerDeptIds.Contains(ticket.DepartmentId))
        ModelState.AddModelError("DepartmentId", "You cannot create a ticket for a department you manage.");

    
    if (!ModelState.IsValid)
    {
        var departmentsQuery = _context.Departments.Where(d => d.ParentDepartmentId != null);

        if (role == UserRole.User && userDeptId.HasValue)
        {
            
            var userDept = await _context.Departments
                .Where(d => d.Id == userDeptId.Value)
                .Select(d => new { d.Id, d.ParentDepartmentId })
                .FirstOrDefaultAsync();

            if (userDept != null)
            {
                var userRootId = userDept.ParentDepartmentId ?? userDept.Id;
                departmentsQuery = departmentsQuery.Where(d => d.ParentDepartmentId != userRootId);
            }
        }

        if (role == UserRole.Manager && managerDeptIds.Any())
            departmentsQuery = departmentsQuery.Where(d => !managerDeptIds.Contains(d.Id));

        var departments = await departmentsQuery.OrderBy(d => d.Name).ToListAsync();

        ViewData["DepartmentId"] = new SelectList(departments, "Id", "Name", ticket.DepartmentId);
        return View(ticket);
    }

    ticket.Status = TicketStatus.New;

    var userId = GetCurrentUserId();
    ticket.CreatedByUserId = userId;
    ticket.CreatedBy = GetCurrentUserName(); 

    ticket.CreatedAt = DateTime.UtcNow;

    // ✅ Fix: always set FromDepartmentId for USER (otherwise stop)
    ticket.FromDepartmentId = await GetSenderDepartmentIdAsync();

    if (role == UserRole.User && !ticket.FromDepartmentId.HasValue)
    {
        TempData["Error"] = "Your user does not have a department assigned. Please contact admin.";
        return RedirectToAction(nameof(Index));
    }

    ticket.Priority = null;
    ticket.AssignedUserId = null;

    _context.Tickets.Add(ticket);
    await _context.SaveChangesAsync();

    var year = DateTime.UtcNow.Year;
    ticket.ReferenceNumber = $"TS-{year}-{ticket.Id:D6}";
    await _context.SaveChangesAsync();

   
    if (files != null && files.Count > 0)
    {
        var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var ticketFolder = Path.Combine(rootPath, "uploads", ticket.Id.ToString());
        Directory.CreateDirectory(ticketFolder);

        foreach (var file in files)
        {
            if (file.Length <= 0) continue;

            var originalName = Path.GetFileName(file.FileName);
            var uniqueName = $"{Guid.NewGuid()}_{originalName}";
            var filePath = Path.Combine(ticketFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = Path.Combine("uploads", ticket.Id.ToString(), uniqueName).Replace("\\", "/");

            _context.TicketAttachments.Add(new TicketAttachment
            {
                TicketId = ticket.Id,
                FileName = originalName,
                FilePath = relativePath,
                ContentType = file.ContentType
            });
        }

        await _context.SaveChangesAsync();
    }

    return RedirectToAction(nameof(Index));
}

        public async Task<IActionResult> Details(int id, string? from = null)
        {
            var ticket = await _context.Tickets
                .Include(t => t.Department)
                .Include(t => t.FromDepartment)
                .Include(t => t.AssignedUser)
                .Include(t => t.Attachments)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            var role = GetCurrentRole();
            var currentUserName = GetCurrentUserName();
            var currentUserId = GetCurrentUserId();
            var currentDeptId = GetCurrentDepartmentId();

            ViewData["CurrentRole"] = role;
            ViewBag.From = from;
            ViewBag.CurrentUserId = currentUserId;
            ViewBag.CurrentDepartmentId = currentDeptId;

            if (currentUserId.HasValue)
            {
                ViewBag.CanManage = await _context.AppUsers
                    .Where(u => u.Id == currentUserId.Value)
                    .Select(u => u.CanManageDeptTickets)
                    .FirstOrDefaultAsync();
            }
            else
            {
                ViewBag.CanManage = false;
            }

            ViewBag.History = await _context.TicketHistories
                .Where(h => h.TicketId == id)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();

            if (TempData["Error"] != null)
                ModelState.AddModelError("", TempData["Error"]!.ToString()!);

            if (role == UserRole.Admin)
            {
                ViewBag.AssignableUsers = Enumerable.Empty<AppUser>();
                ViewBag.ManagerCanAct = false;
            }
            else if (role == UserRole.Manager)
            {
                var managedDeptIds = GetManagerDeptIds();
                var isDeptAllowed = managedDeptIds.Contains(ticket.DepartmentId);
                var isCreator = string.Equals(ticket.CreatedBy, currentUserName, StringComparison.OrdinalIgnoreCase);

                if (!isDeptAllowed && !isCreator)
                    return Forbidden();

                ViewBag.ManagerCanAct = isDeptAllowed;

                if (isDeptAllowed)
                {
                    var users = await _context.AppUsers
                        .Where(u => u.IsActive && u.Role == UserRole.User && u.DepartmentId == ticket.DepartmentId)
                        .OrderBy(u => u.UserName)
                        .ToListAsync();

                    ViewBag.AssignableUsers = users;
                }
                else
                {
                    ViewBag.AssignableUsers = Enumerable.Empty<AppUser>();
                }
            }
            else
            {
                var isCreator = string.Equals(ticket.CreatedBy, currentUserName, StringComparison.OrdinalIgnoreCase);
                var isAssigned = currentUserId.HasValue && ticket.AssignedUserId.HasValue && ticket.AssignedUserId.Value == currentUserId.Value;
                var isSameDept = currentDeptId.HasValue && ticket.DepartmentId == currentDeptId.Value;

                if (!isCreator && !isAssigned && !isSameDept)
                    return Forbidden();

                ViewBag.AssignableUsers = Enumerable.Empty<AppUser>();
                ViewBag.ManagerCanAct = false;
            }

            if (IsAjaxRequest())
                return View("Details", ticket);

            return View(ticket);
        }

           // ========================= ACTION: User Manage (Self-Assign) =========================
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Manage(int id)
{
    var role = GetCurrentRole();
    if (role != UserRole.User) return Forbidden();

    var userId = GetCurrentUserId();
    var deptId = GetCurrentDepartmentId();
    if (!userId.HasValue || !deptId.HasValue) return Forbidden();

    var canManage = await _context.AppUsers
        .Where(u => u.Id == userId.Value)
        .Select(u => u.CanManageDeptTickets)
        .FirstOrDefaultAsync();

    if (!canManage) return Forbidden();

    var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    if (ticket == null) return NotFound();

    
    if (ticket.DepartmentId != deptId.Value) return Forbidden();
    if (ticket.Status != TicketStatus.New) return Forbidden();

    var oldStatus = ticket.Status;

    
    ticket.AssignedUserId = userId.Value;
    ticket.Status = TicketStatus.InProgress;

    _context.TicketHistories.Add(new TicketHistory
    {
        TicketId = ticket.Id,
        OldStatus = oldStatus,
        NewStatus = ticket.Status,
        ChangedAt = DateTime.UtcNow,
        ChangedBy = GetCurrentUserName(),
        Role = role.ToString(),
        Comment = "User managed ticket (self-assign) and started processing."
    });

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Details), new { id, from = "dashboard" });
}

// ========================= ACTION: Manager Approve & Assign =========================
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ApproveAndAssign(int id, int assignedUserId, TicketPriority priority)
{
    var role = GetCurrentRole();
    if (role != UserRole.Manager) return Forbidden();

    var managedDeptIds = GetManagerDeptIds();
    if (managedDeptIds.Count == 0) return Forbidden();

    var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    if (ticket == null) return NotFound();

    if (!managedDeptIds.Contains(ticket.DepartmentId)) return Forbidden();

    
    if (ticket.Status != TicketStatus.New)
    {
        TempData["Error"] = "This ticket is not in New status.";
        return RedirectToAction(nameof(Details), new { id });
    }

    
    var userOk = await _context.AppUsers.AnyAsync(u =>
        u.Id == assignedUserId &&
        u.IsActive &&
        u.Role == UserRole.User &&
        u.DepartmentId == ticket.DepartmentId
    );

    if (!userOk)
    {
        TempData["Error"] = "Selected user is not valid for this ticket department.";
        return RedirectToAction(nameof(Details), new { id });
    }

    var oldStatus = ticket.Status;

    ticket.Priority = priority;
    ticket.AssignedUserId = assignedUserId;
    ticket.Status = TicketStatus.AssignedToDepartment;

    _context.TicketHistories.Add(new TicketHistory
    {
        TicketId = ticket.Id,
        OldStatus = oldStatus,
        NewStatus = ticket.Status,
        ChangedAt = DateTime.UtcNow,
        ChangedBy = GetCurrentUserName(),
        Role = role.ToString(),
        Comment = $"Approved and assigned to userId={assignedUserId}, priority={priority}."
    });

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Details), new { id, from = "manager" });
}

// ========================= ACTION: Manager Solve (Start Working without assigning) =========================
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SolveMyself(int id)
{
    var role = GetCurrentRole();
    if (role != UserRole.Manager) return Forbidden();

    var managedDeptIds = GetManagerDeptIds();
    if (managedDeptIds.Count == 0) return Forbidden();

    var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    if (ticket == null) return NotFound();

    if (!managedDeptIds.Contains(ticket.DepartmentId)) return Forbidden();

    
    if (ticket.Status != TicketStatus.New)
    {
        TempData["Error"] = "This ticket is not in New status.";
        return RedirectToAction(nameof(Details), new { id });
    }

    var oldStatus = ticket.Status;

    ticket.Status = TicketStatus.InProgress;
    ticket.AssignedUserId = null; 

    _context.TicketHistories.Add(new TicketHistory
    {
        TicketId = ticket.Id,
        OldStatus = oldStatus,
        NewStatus = ticket.Status,
        ChangedAt = DateTime.UtcNow,
        ChangedBy = GetCurrentUserName(),
        Role = role.ToString(),
        Comment = "Manager started working without assigning."
    });

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Details), new { id, from = "manager" });
}

// ========================= ACTION: Manager Close ticket (when InProgress and no AssignedUser) =========================
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ManagerCloseSolved(int id)
{
    var role = GetCurrentRole();
    if (role != UserRole.Manager) return Forbidden();

    var managedDeptIds = GetManagerDeptIds();
    if (managedDeptIds.Count == 0) return Forbidden();

    var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    if (ticket == null) return NotFound();

    if (!managedDeptIds.Contains(ticket.DepartmentId)) return Forbidden();

    
    if (ticket.Status != TicketStatus.InProgress || ticket.AssignedUserId != null)
    {
        TempData["Error"] = "Ticket cannot be closed in its current state.";
        return RedirectToAction(nameof(Details), new { id });
    }

    var oldStatus = ticket.Status;

    ticket.Status = TicketStatus.Closed;

    _context.TicketHistories.Add(new TicketHistory
    {
        TicketId = ticket.Id,
        OldStatus = oldStatus,
        NewStatus = ticket.Status,
        ChangedAt = DateTime.UtcNow,
        ChangedBy = GetCurrentUserName(),
        Role = role.ToString(),
        Comment = "Manager closed the ticket."
    });

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Details), new { id, from = "manager" });
}

// ========================= ACTION: ChangeStatus (Assigned User actions + Manager reject) =========================
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ChangeStatus(int id, TicketStatus newStatus, string? comment = null)
{
    var role = GetCurrentRole();
    var userId = GetCurrentUserId();
    var userName = GetCurrentUserName();

    if (!userId.HasValue) return Forbidden();

    var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
    if (ticket == null) return NotFound();

    
    if (role == UserRole.User)
    {
        if (!ticket.AssignedUserId.HasValue || ticket.AssignedUserId.Value != userId.Value)
            return Forbidden();

        
        if (ticket.Status == TicketStatus.AssignedToDepartment && newStatus != TicketStatus.InProgress)
            return Forbidden();

        if (ticket.Status == TicketStatus.InProgress && newStatus != TicketStatus.Closed)
            return Forbidden();

        if (ticket.Status != TicketStatus.AssignedToDepartment && ticket.Status != TicketStatus.InProgress)
            return Forbidden();
    }
    else if (role == UserRole.Manager)
    {
        var managedDeptIds = GetManagerDeptIds();
        if (!managedDeptIds.Contains(ticket.DepartmentId)) return Forbidden();

        
        if (!(ticket.Status == TicketStatus.New && newStatus == TicketStatus.Closed))
            return Forbidden();

        if (string.IsNullOrWhiteSpace(comment))
        {
            TempData["Error"] = "Reject reason is required.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
    else
    {
        return Forbidden();
    }

    var oldStatus = ticket.Status;
    ticket.Status = newStatus;

    _context.TicketHistories.Add(new TicketHistory
    {
        TicketId = ticket.Id,
        OldStatus = oldStatus,
        NewStatus = newStatus,
        ChangedAt = DateTime.UtcNow,
        ChangedBy = userName,
        Role = role.ToString(),
        Comment = comment
    });

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Details), new { id });
}
 
    }
}
