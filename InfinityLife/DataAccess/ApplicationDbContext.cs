using InfinityLife.Models;
using Microsoft.EntityFrameworkCore;

namespace InfinityLife.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Employee> Employee { get; set; }
        public DbSet<EmployeeRole> EmployeeRole { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Employee entity
            modelBuilder.Entity<Employee>()
                .HasKey(e => e.EmpId);

            modelBuilder.Entity<Employee>()
                .Property(e => e.EmpEmail)
                .IsRequired();

            // Configure EmployeeRole entity
            modelBuilder.Entity<EmployeeRole>()
                .HasKey(r => r.EmpRoleId);
        }
    }
}