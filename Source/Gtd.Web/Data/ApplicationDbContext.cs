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
        }

        public DbSet<TaskDto> Tasks { get; set; }
    }
}
