using Microsoft.EntityFrameworkCore;
using task_manager_api.Models;

namespace task_manager_api.Data
{
    public class TaskManagerDbContext : DbContext
    {
        public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.ToTable("Tasks");
                entity.HasKey(e => e.TaskId);

                entity.Property(e => e.Title)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.IsCompleted)
                      .IsRequired();

                entity.Property(e => e.CreatedAt)
                      .IsRequired();
            });
        }
    }
}
