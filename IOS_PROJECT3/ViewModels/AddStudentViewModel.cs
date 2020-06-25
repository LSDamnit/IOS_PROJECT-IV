using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.ViewModels
{
    public class AddStudentViewModel
    {

        
        [Required]
        public string Email { get; set; }
        public string FIO { get; set; }
        public string CurrentSpec { get; set; }
        public string CurrentSpecId { get; set; }
        public string TargetSpecId { get; set; }
        public IList<EUser> AvailableStudents { get; set; }
        public DBMergedContext context { get; set; }

        public async Task FillDataAsync()
        {
            var user = await (from u in context.Users where u.Email.ToLower() == Email.ToLower() select u).FirstOrDefaultAsync();
            var spec = await (from s in context.Specialities.Include(s => s.Students)
                              where s.Students.Contains(user)
                              select s).FirstOrDefaultAsync();
            if(user!=null)
            {
                FIO = user.FIO;
                if (spec != null)
                {
                    CurrentSpec = spec.Name;
                    CurrentSpecId = spec.Id.ToString();
                }
                
            }
            
        }
    }
}
