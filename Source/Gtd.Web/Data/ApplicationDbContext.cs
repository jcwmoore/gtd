using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Gtd.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Tasks
            builder.Entity<TaskDto>().ToTable("Tasks");
            builder.Entity<TaskDto>().HasKey(t => t.Id);
            builder.Entity<TaskDto>().Property(t => t.Created).IsRequired();
            builder.Entity<TaskDto>().Property(t => t.Updated).IsRequired();
            builder.Entity<TaskDto>().Property(t => t.Title).IsRequired();            
            builder.Entity<TaskDto>().Property(t => t.UserId).IsRequired();

            // Projects
            builder.Entity<ProjectDto>().ToTable("Projects");
            builder.Entity<ProjectDto>().HasKey(p => p.Id);
            builder.Entity<ProjectDto>().HasMany(p => p.Tasks).WithOne(t => t.Project);
            builder.Entity<ProjectDto>().Property(p => p.Created).IsRequired();
            builder.Entity<ProjectDto>().Property(p => p.Updated).IsRequired();
            builder.Entity<ProjectDto>().Property(p => p.Title).IsRequired();     
        }

        public DbSet<TaskDto> Tasks { get; set; }

        public DbSet<ProjectDto> Projects { get; set; }
    }
}
