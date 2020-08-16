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
            builder.Entity<Models.TaskModel>().ToTable("Tasks");
            builder.Entity<Models.TaskModel>().HasKey(t => t.Id);
            builder.Entity<Models.TaskModel>().Property(t => t.Created).IsRequired();
            builder.Entity<Models.TaskModel>().Property(t => t.Updated).IsRequired();
            builder.Entity<Models.TaskModel>().Property(t => t.Title).IsRequired();            
            builder.Entity<Models.TaskModel>().Property(t => t.UserId).IsRequired();
        }

        public DbSet<Models.TaskModel> Tasks { get; set; }
    }
}
