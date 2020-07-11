using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace IOS_PROJECT3.Models
{
    public class DBMergedContext:IdentityDbContext<EUser>
    {
        public DbSet<EInstitution> Institutions { get; set; }
        public DbSet<EDepartment> Departments { get; set; }
        public DbSet<ESpeciality> Specialities { get; set; }
        public DbSet<EDiscipline> Disciplines { get; set; }
        public DbSet<EFile> Files { get; set; }
        public DbSet<EWeekSchedule> WeekSchedules { get; set; }
        public DbSet<EDaySchedule> DaySchedules { get; set; }
        public DbSet<EScheduleItem> ScheduleItems { get; set; }
        public DbSet<EForumNode> ForumNodes { get; set; }
        public DbSet<EForumEndopoint> ForumEndpoints { get; set; }
        public DbSet<EForumFile> ForumFiles { get; set; }
        public DbSet<EGrant> Grants { get; set; }
        public DbSet<ERolesToGrants> RolesToGrants { get; set; }


        public DBMergedContext(DbContextOptions<DBMergedContext> options)
           : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EInstitution>()
                .HasMany(d => d.Departments)
                .WithOne(i=>i.Institution)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EDepartment>()
                .HasMany(d => d.Specialities)
                .WithOne(d=>d.Department)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ESpeciality>()
                .HasMany(d => d.Disciplines)
                .WithOne(d=>d.Speciality)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ESpeciality>()
                .HasMany(s => s.Students)
                .WithOne(s=>s.Speciality)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<EDiscipline>()
                .HasMany(d => d.Files)
                .WithOne(d=>d.Discipline)
                .OnDelete(DeleteBehavior.Cascade);

            //----Schedule-----
            modelBuilder.Entity<EWeekSchedule>()
                .HasMany(d => d.Schedule)
                .WithOne(d => d.WeekSchedule)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EDaySchedule>()
                .HasMany(d => d.DisciplinesForDay)
                .WithOne(d => d.DaySchedule)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ESpeciality>()
                .HasMany(s => s.Schedules)
                .WithOne(s => s.Speciality)
                .OnDelete(DeleteBehavior.Cascade);
            //-----Forum------
            modelBuilder.Entity<EForumNode>()
                .HasMany(s => s.ChildNodes)
                .WithOne(p => p.ParentNode)
                .OnDelete(DeleteBehavior.NoAction);//<---реализовать собственное каскадное удаление
            modelBuilder.Entity<EForumEndopoint>()
                .HasMany(f => f.PinnedFiles)
                .WithOne(e => e.ForumEndpoint)
                .OnDelete(DeleteBehavior.Cascade);
            //-------Grants----
            modelBuilder.Entity<ERolesToGrants>()
                .HasKey(p => new { p.RoleId, p.GrantId });
        }
    }
}
