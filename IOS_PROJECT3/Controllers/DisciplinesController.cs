using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.Controllers
{
    public class DisciplinesController : Controller
    {
        private DBMergedContext DBContext;
        UserManager<EUser> userManager;

        public DisciplinesController(DBMergedContext context, UserManager<EUser> userManager)
        {
            DBContext = context;
            this.userManager = userManager;
        }
        public async Task<IActionResult> Index(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines).Include(s => s.Students)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
            if (spec != null)
            {
                var dis = await (from d in DBContext.Disciplines.Include(t => t.Teacher)
                                 where spec.Disciplines.Contains(d)
                                 select d).ToListAsync();
                var dep = await (from i in DBContext.Departments.Include(d => d.Specialities)
                                 where i.Specialities.Contains(spec)
                                 select i).FirstOrDefaultAsync();
                var inst = await (from i in DBContext.Institutions.Include(d => d.Departments).Include(m => m.Manager)
                                  where i.Departments.Contains(dep)
                                  select i).FirstOrDefaultAsync();

                var model = new DisciplinesViewModel()
                {
                    SpecialityId = spec.Id.ToString(),
                    SpecialityName = spec.Name,
                    InstId = inst.Id.ToString(),
                    InstManagerId = inst.Manager.Id,
                    InstManagerEmail = inst.Manager.Email,
                    Disciplines = dis,
                    Students = spec.Students
                };
                return View(model);

            }
            return View();//сделать редирект на страницу с ошибкой
        }

        public async Task<IActionResult> Create(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
            if (spec != null)
            {
                var model = new CreateDisciplineViewModel()
                {
                    SpecialityId = spec.Id.ToString(),
                    AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher")
                };
                return View(model);
            }
            ModelState.AddModelError("Create", "No such speciality");
            return RedirectToAction("Index");//untested
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDisciplineViewModel model)
        {
            var teacher = await userManager.FindByIdAsync(model.TeacherId);
            var spec = await (from s in DBContext.Specialities.Include(d => d.Disciplines)
                              where s.Id.ToString() == model.SpecialityId
                              select s).FirstOrDefaultAsync();
            var name = model.Name;
            if (!String.IsNullOrWhiteSpace(name) && teacher != null && !String.IsNullOrWhiteSpace(model.ExamType))
            {
                EDiscipline disc = new EDiscipline()
                {
                    Name = name,
                    LectionH = model.LectionH,
                    PracticeH = model.PracticeH,
                    ExamType = model.ExamType,
                    Teacher = teacher

                };
                DBContext.Disciplines.Add(disc);
                spec.Disciplines.Add(disc);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { SpecId = spec.Id });
            }
            ModelState.AddModelError("Create", "Error in disc create");
            return View(model);
        }

        public async Task<IActionResult> Edit(string DiscId)
        {
            var disc = await (from d in DBContext.Disciplines.Include(h => h.Teacher) where d.Id.ToString() == DiscId select d).FirstOrDefaultAsync();
            if (disc != null)
            {
                var spec = await (from i in DBContext.Specialities.Include(d => d.Disciplines)
                                  where i.Disciplines.Contains(disc)
                                  select i).FirstOrDefaultAsync();
                var model = new EditDisciplineViewModel()
                {
                    Name = disc.Name,
                    DisciplineId = disc.Id.ToString(),
                    SpecialityId = spec.Id.ToString(),
                    LectionH = disc.LectionH,
                    PracticeH = disc.PracticeH,
                    ExamType = disc.ExamType,
                    TeacherId = disc.Teacher.Id,
                    AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher")

                };
                return View(model);
            }
            return View();//сделать редирект на ошибку
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDisciplineViewModel model)
        {
            var teacher = await userManager.FindByIdAsync(model.TeacherId);
            string name = model.Name;

            if (teacher != null && !String.IsNullOrWhiteSpace(name)&& !String.IsNullOrWhiteSpace(model.ExamType))
            {
                var disc = await (from d in DBContext.Disciplines.Include(h => h.Teacher)
                                 where d.Id.ToString() == model.DisciplineId
                                 select d).FirstOrDefaultAsync();

                DBContext.Update(disc).Entity.Teacher = teacher;
                DBContext.Update(disc).Entity.Name = name;
                DBContext.Update(disc).Entity.ExamType = model.ExamType;
                DBContext.Update(disc).Entity.LectionH = model.LectionH;
                DBContext.Update(disc).Entity.PracticeH = model.PracticeH;

                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { SpecId = model.SpecialityId });
            }
            ModelState.AddModelError("Edit", "Error in department edit");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            var disc = await (from d in DBContext.Disciplines where d.Id.ToString() == Id select d).FirstOrDefaultAsync();
            if (disc != null)
            {
                var spec = await (from i in DBContext.Specialities.Include(d => d.Disciplines)
                                  where i.Disciplines.Contains(disc)
                                  select i).FirstOrDefaultAsync();
                DBContext.Remove(disc);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { SpecId = spec.Id });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}