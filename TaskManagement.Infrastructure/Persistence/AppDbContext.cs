using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamUser> TeamUsers => Set<TeamUser>();
    public DbSet<WorkLog> WorkLogs => Set<WorkLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TeamUser many-to-many configuration
        modelBuilder.Entity<TeamUser>()
            .HasKey(tu => new { tu.TeamId, tu.UserId });

        modelBuilder.Entity<TeamUser>()
            .HasOne(tu => tu.Team)
            .WithMany(t => t.Members)
            .HasForeignKey(tu => tu.TeamId);

        modelBuilder.Entity<TeamUser>()
            .HasOne(tu => tu.User)
            .WithMany() 
            .HasForeignKey(tu => tu.UserId);

        // TaskItem -> Project (one-to-many)
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId);

        // TaskItem optional AssignedUser
        modelBuilder.Entity<TaskItem>()
            .HasOne(t => t.AssignedUser)
            .WithMany() 
            .HasForeignKey(t => t.AssignedUserId)
            .OnDelete(DeleteBehavior.SetNull);

        // WorkLog -> User (many-to-one)
        modelBuilder.Entity<WorkLog>()
            .HasOne(wl => wl.User)
            .WithMany(u => u.WorkLogs)
            .HasForeignKey(wl => wl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // WorkLog -> TaskItem (many-to-one)
        modelBuilder.Entity<WorkLog>()
            .HasOne(wl => wl.Task)
            .WithMany(t => t.WorkLogs)
            .HasForeignKey(wl => wl.TaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
