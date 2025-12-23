using ITTicketSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ITTicketSystem.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // إذا فيه أقسام لا تعيد الزراعة
            if (context.Departments.Any())
            {
                Console.WriteLine("SeedData: Departments already exist, skipping seeding.");
                return;
            }

            Console.WriteLine("SeedData: Seeding departments (3 main + 6 sub)...");

            // 1) الأقسام الرئيسية
            var hr = new Department { Name = "HR" };
            var it = new Department { Name = "IT" };
            var finance = new Department { Name = "Finance" };

            context.Departments.AddRange(hr, it, finance);
            context.SaveChanges();

            // 2) الأقسام الفرعية
            var children = new List<Department>
            {
                new Department { Name = "HR Training", ParentDepartment = hr },
                new Department { Name = "HR Management", ParentDepartment = hr },

                new Department { Name = "IT Training", ParentDepartment = it },
                new Department { Name = "IT Management", ParentDepartment = it },

                new Department { Name = "Finance Training", ParentDepartment = finance },
                new Department { Name = "Finance Management", ParentDepartment = finance }
            };

            context.Departments.AddRange(children);
            context.SaveChanges();

            Console.WriteLine("SeedData: Seeding completed.");
        }
    }
}
