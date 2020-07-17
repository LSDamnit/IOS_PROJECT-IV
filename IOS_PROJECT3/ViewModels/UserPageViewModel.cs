using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.Grants;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IOS_PROJECT3.ViewModels
{
    public class UserPageViewModel
    {
        public string UserFIO { get; set; }
        public string UserId { get; set; }
        
        public IList<EInstitution> OwnInstitutions_M { get; set; }
        public IList<EDiscipline> OwnDiscilpines_T { get; set; }
        public ESpeciality OwnSpeciality_S { get; set; }

        private DBMergedContext DBContext;
        public List<string> userGrants;
        public List<string> userRoles;
        //---SecuritySettingsChangeForm
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
        public string NewPasswordConfirm { get; set; }
        public bool NotifyOnLogin { get; set; }
        public bool BlockOnFails { get; set; }
        //-----------------
        public async Task CheckAsync()
        {
            var idRoles = await (from ro in DBContext.UserRoles
                               where ro.UserId == UserId
                               select ro).ToListAsync();
            var user = await (from u in DBContext.Users
                              where u.Id == UserId
                              select u).FirstOrDefaultAsync();
            if (user.BlockOnFailedLogins == 1)
                BlockOnFails = true;
            if (user.NotifyOnLogins == 1)
                NotifyOnLogin = true;
            userRoles = new List<string>();
            foreach(var r in idRoles)
            {
                var rname = await (from ro in DBContext.Roles
                                   where ro.Id == r.RoleId
                                   select ro.Name).FirstOrDefaultAsync();
                userRoles.Add(rname);
            }
            if(userRoles.Contains("Student"))
            {
                //var user = await (from u in DBContext.Users where u.Id == UserId select u).FirstOrDefaultAsync();
                OwnSpeciality_S = await (from sp in DBContext.Specialities.Include(s => s.Students)
                                         where sp.Students.Contains(user)
                                         select sp).FirstOrDefaultAsync();
            }
            if (userRoles.Contains("Teacher"))
            {
                OwnDiscilpines_T = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                          where di.Teacher.Id == UserId
                                          select di).ToListAsync();
            }
            if (userRoles.Contains("Manager"))
            {
                OwnInstitutions_M = await (from i in DBContext.Institutions.Include(m => m.Manager)
                                           where i.Manager.Id == UserId
                                           select i).ToListAsync();
            }
            
        }
        public UserPageViewModel()
        {

        }
        public UserPageViewModel(DBMergedContext context)
        {
            DBContext = context;
            
           
        }
    }
}
