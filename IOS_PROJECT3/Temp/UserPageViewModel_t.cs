using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IOS_PROJECT3.Models;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.ViewModels
{
    public class UserPageViewModel_t
    {
        public string UserFIO { get; set; }
        public string UserId { get; set; }
        public IList<EInstitution> Institutions { get; }
		public IList<EDepartment> Departments { get; }
		public IList<EInstitution> OwnInstitutions_M { get; set; }
        public IList<EDiscipline> OwnDiscilpines_T { get; set; }
        public ESpeciality OwnSpeciality_S { get; set; }

        private DBMergedContext DBContext;
        
        public async Task CheckAblesAsync()
        {
            var idRoles = await (from ro in DBContext.UserRoles
                               where ro.UserId == UserId
                               select ro).ToListAsync();
            var stRoles = new List<string>();
            foreach(var r in idRoles)
            {
                var rname = await (from ro in DBContext.Roles
                                   where ro.Id == r.RoleId
                                   select ro.Name).FirstOrDefaultAsync();
                stRoles.Add(rname);
            }
            if(stRoles.Contains("Student"))
            {
                var user = await (from u in DBContext.Users where u.Id == UserId select u).FirstOrDefaultAsync();
                OwnSpeciality_S = await (from sp in DBContext.Specialities.Include(s => s.Students)
                                         where sp.Students.Contains(user)
                                         select sp).FirstOrDefaultAsync();
            }
            if (stRoles.Contains("Teacher"))
            {
                OwnDiscilpines_T = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                          where di.Teacher.Id == UserId
                                          select di).ToListAsync();
            }
            if (stRoles.Contains("Manager"))
            {
                OwnInstitutions_M = await (from i in DBContext.Institutions.Include(m => m.Manager)
                                           where i.Manager.Id == UserId
                                           select i).ToListAsync();
            }
            
        }
        public UserPageViewModel_t(DBMergedContext context)
        {
            DBContext = context;
            Institutions = DBContext.Institutions.ToList();
			Departments = DBContext.Departments.ToList();
        }
    }
}
