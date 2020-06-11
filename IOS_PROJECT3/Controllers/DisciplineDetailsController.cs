using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.Controllers
{
    public class DisciplineDetailsController : Controller
    {
        DBMergedContext DBContext;
        public DisciplineDetailsController(DBMergedContext context)
        {
            DBContext = context;
        }
        public async Task<IActionResult> Index(string DiscId)
        {
            var disc = await (from di in DBContext.Disciplines.Include(t=>t.Teacher).Include(f=>f.Files) where di.Id.ToString() == DiscId select di).FirstOrDefaultAsync();
            var spec = await (from s in DBContext.Specialities.Include(s => s.Disciplines)
                              where s.Disciplines.Contains(disc)
                              select s).FirstOrDefaultAsync();
            var dep = await (from d in DBContext.Departments.Include(s => s.Specialities) where d.Specialities.Contains(spec)
                             select d).FirstOrDefaultAsync();
            var inst = await (from i in DBContext.Institutions.Include(s => s.Departments).Include(m=>m.Manager)
                              where i.Departments.Contains(dep)
                              select i).FirstOrDefaultAsync();
            var model = new DisciplineDetailsViewModel()
            {
                DisciplineId=disc.Id.ToString(),
                DisciplineName=disc.Name,
                TeacherId=disc.Teacher.Id,
                TeacherEmail=disc.Teacher.Email,
                InstId=inst.Id.ToString(),
                InstManagerId=inst.Manager.Id.ToString(),
                InstManagerEmail=inst.Manager.Email,
                Files=disc.Files,
                //DisciplineInfo=String.Join("<p></p>",disc.About.Split("\n")),
                DisciplineInfo=disc.About,
                LecH=disc.LectionH.ToString(),
                PracH=disc.PracticeH.ToString(),
                ExamType=disc.ExamType,
                TeacherName=disc.Teacher.FIO
            };
            return View(model);
        }
    }
}