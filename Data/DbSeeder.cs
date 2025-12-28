using ITTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTicketSystem.Data
{
    public static class UsersSeeder
    {
        public static async Task SeedDefaultUsersAndManagersAsync(ApplicationDbContext db)
        {
            
            var departments = await db.Departments.ToListAsync();
            if (!departments.Any()) return;

            Department? Dept(string name) => departments.FirstOrDefault(d => d.Name == name);

            var hrTraining = Dept("HR Training");
            var hrManagement = Dept("HR Management");
            var itTraining = Dept("IT Training");
            var itManagement = Dept("IT Management");
            var finTraining = Dept("Finance Training");
            var finManagement = Dept("Finance Management");

            var required = new[] { hrTraining, hrManagement, itTraining, itManagement, finTraining, finManagement };
            if (required.Any(d => d == null)) return;

            
            var referencedUserIds = await db.Tickets
                .Where(t => t.CreatedByUserId.HasValue)
                .Select(t => t.CreatedByUserId!.Value)
                .Distinct()
                .ToListAsync();

            
            var linksToRemove = await db.ManagerDepartments.ToListAsync();
            if (linksToRemove.Any())
            {
                db.ManagerDepartments.RemoveRange(linksToRemove);
                await db.SaveChangesAsync();
            }

           
            var usersToRemove = await db.AppUsers
                .Where(u => u.UserName != "admin" && !referencedUserIds.Contains(u.Id))
                .ToListAsync();

            if (usersToRemove.Any())
            {
                db.AppUsers.RemoveRange(usersToRemove);
                await db.SaveChangesAsync();
            }

           
            var adminExists = await db.AppUsers.AnyAsync(u => u.UserName == "admin");
            if (!adminExists)
            {
                db.AppUsers.Add(new AppUser
                {
                    UserName = "admin",
                    PasswordHash = PasswordHelper.Hash("1234"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    DepartmentId = null
                });
                await db.SaveChangesAsync();
            }

            var passwordHash = PasswordHelper.Hash("1234");

            // Helper: create or update user by username
            async Task<AppUser> UpsertUser(string userName, UserRole role, int? departmentId)
            {
                var u = await db.AppUsers.FirstOrDefaultAsync(x => x.UserName == userName);
                if (u == null)
                {
                    u = new AppUser
                    {
                        UserName = userName,
                        PasswordHash = passwordHash,
                        Role = role,
                        IsActive = true,
                        DepartmentId = departmentId
                    };
                    db.AppUsers.Add(u);
                    await db.SaveChangesAsync();
                }
                else
                {
                    
                    u.Role = role;
                    u.IsActive = true;
                    u.DepartmentId = departmentId;
                    
                    u.PasswordHash = passwordHash;
                    await db.SaveChangesAsync();
                }
                return u;
            }

            
            await UpsertUser("hr.training", UserRole.User, hrTraining!.Id);
            await UpsertUser("hr.management", UserRole.User, hrManagement!.Id);

            await UpsertUser("it.training", UserRole.User, itTraining!.Id);
            await UpsertUser("it.management", UserRole.User, itManagement!.Id);

            await UpsertUser("fin.training", UserRole.User, finTraining!.Id);
            await UpsertUser("fin.management", UserRole.User, finManagement!.Id);

          
            var hrMgr = await UpsertUser("mgr.hr", UserRole.Manager, null);
            var itMgr = await UpsertUser("mgr.it", UserRole.Manager, null);
            var finMgr = await UpsertUser("mgr.finance", UserRole.Manager, null);

            
            var newLinks = new List<ManagerDepartment>
            {
                new ManagerDepartment { ManagerUserId = hrMgr.Id,  DepartmentId = hrTraining!.Id },
                new ManagerDepartment { ManagerUserId = hrMgr.Id,  DepartmentId = hrManagement!.Id },

                new ManagerDepartment { ManagerUserId = itMgr.Id,  DepartmentId = itTraining!.Id },
                new ManagerDepartment { ManagerUserId = itMgr.Id,  DepartmentId = itManagement!.Id },

                new ManagerDepartment { ManagerUserId = finMgr.Id, DepartmentId = finTraining!.Id },
                new ManagerDepartment { ManagerUserId = finMgr.Id, DepartmentId = finManagement!.Id },
            };

            db.ManagerDepartments.AddRange(newLinks);
            await db.SaveChangesAsync();
        }
    }
}
