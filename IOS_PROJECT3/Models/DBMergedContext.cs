using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.Models
{
    public class DBMergedContext:IdentityDbContext<EUser>
    {
        public DbSet<EInstitution> Institutions { get; set; }
        public DbSet<EDepartment> Departments { get; set; }
        public DbSet<ESpeciality> Specialities { get; set; }
        public DbSet<EDiscipline> Disciplines { get; set; }
        public DbSet<EFile> Files { get; set; }       
        //public DbSet<RegFile> RegistrationFiles { get; set; }

        public DBMergedContext(DbContextOptions<DBMergedContext> options)
           : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
