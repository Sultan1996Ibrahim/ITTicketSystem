using ITTicketSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ITTicketSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Ticket> Tickets { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<TicketHistory> TicketHistories { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<TicketAttachment> TicketAttachments { get; set; } = null!;
        public DbSet<ManagerDepartment> ManagerDepartments { get; set; } = null!;
        public DbSet<AppUser> AppUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ManagerDepartment (many-to-many)
            modelBuilder.Entity<ManagerDepartment>()
                .HasKey(x => new { x.ManagerUserId, x.DepartmentId });

            modelBuilder.Entity<ManagerDepartment>()
                .HasOne(x => x.ManagerUser)
                .WithMany(u => u.ManagedDepartments)
                .HasForeignKey(x => x.ManagerUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ManagerDepartment>()
                .HasOne(x => x.Department)
                .WithMany(d => d.Managers)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ticket -> AssignedUser
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // AppUser unique username
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.UserName)
                .IsUnique();

            // Ticket -> Department (target)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Department)
                .WithMany()
                .HasForeignKey(t => t.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ticket -> FromDepartment (sender)
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.FromDepartment)
                .WithMany()
                .HasForeignKey(t => t.FromDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Ticket>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                  .HasForeignKey(t => t.CreatedByUserId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
                  modelBuilder.Entity<AppUser>()
    .HasOne(u => u.Department)
    .WithMany()
    .HasForeignKey(u => u.DepartmentId)
    .OnDelete(DeleteBehavior.SetNull);



        }
    }
}
