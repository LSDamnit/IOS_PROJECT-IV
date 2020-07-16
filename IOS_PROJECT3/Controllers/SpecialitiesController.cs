using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.Grants;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;

namespace IOS_PROJECT3.Controllers
{
    public class SpecialitiesController : Controller
    {
        private DBMergedContext DBContext;
        IWebHostEnvironment environment;
        GrantCheckService checkService;
        // private UserManager<EUser> UserManager;
        public SpecialitiesController(GrantCheckService checkService, DBMergedContext context, IWebHostEnvironment environment)
        {
            this.checkService = checkService;
            DBContext = context;
            this.environment = environment;
            // UserManager = manager;
        }

        public async Task<IActionResult> Index(string DepId)
        {
            var dep = await (from i in DBContext.Departments.Include(s => s.Specialities).Include(h => h.HeadTeacher)
                             where i.Id.ToString() == DepId
                             select i).FirstOrDefaultAsync();

            var specs = await (from d in DBContext.Specialities
                               where dep.Specialities.Contains(d)
                               select d).ToListAsync();

            if (dep != null && specs != null)
            {
                var inst = await (from i in DBContext.Institutions.Include(d => d.Departments).Include(m => m.Manager)
                                  where i.Departments.Contains(dep)
                                  select i).FirstOrDefaultAsync();

                //var manager
                var model = new SpecialitiesViewModel()
                {
                    Specialities = specs,
                    DepartmentId = DepId,
                    DepartmentName = dep.Name,
                    InstId = inst.Id.ToString(),
                    InstManagerId = inst.Manager.Id.ToString(),
                    InstManagerEmail = inst.Manager.Email,
                    HeadTeacherId = dep.HeadTeacher.Id.ToString(),
                    HeadTeacherEmail = dep.HeadTeacher.Email,
                    userGrants = await checkService.getUserGrants(User)
                };
                return View(model);
            }
            else ModelState.AddModelError("Index", "No such department");

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Create(string DepId)
        {
            var id = DepId;
            var model = new CreateSpecialityViewModel()
            {
                //AvailableTeachers = await UserManager.GetUsersInRoleAsync("Teacher"),
                DepId = id,
                userGrants = await checkService.getUserGrants(User)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateSpecialityViewModel model)
        {
            if (ModelState.IsValid) {
                var name = model.Name;
                var dep = await (from i in DBContext.Departments.Include(s => s.Specialities) where i.Id.ToString() == model.DepId select i).FirstOrDefaultAsync();


                var spec = new ESpeciality()
                {
                    Name = name

                };

                dep.Specialities.Add(spec);
                DBContext.Specialities.Add(spec);
                await DBContext.SaveChangesAsync();

                return RedirectToAction("Index", new { DepId = model.DepId });
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string Id)
        {
            var spec = await (from s in DBContext.Specialities where s.Id.ToString() == Id select s).FirstOrDefaultAsync();
            var dep = await (from d in DBContext.Departments.Include(s => s.Specialities)
                             where d.Specialities.Contains(spec)
                             select d).FirstOrDefaultAsync();
            if(spec!=null)
            {
                var model = new EditSpecialityViewModel()
                {
                    SpecId = spec.Id.ToString(),
                    Name = spec.Name,
                    DepId=dep.Id.ToString(),
                    userGrants = await checkService.getUserGrants(User)
                };
                return View(model);
            }
            return View();//<--unpreDICtable
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditSpecialityViewModel model)
        {
            if (ModelState.IsValid) { 
            var spec = await (from s in DBContext.Specialities where s.Id.ToString() == model.SpecId select s).FirstOrDefaultAsync();
            var name = model.Name;
            
                DBContext.Update(spec).Entity.Name = name;
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { DepId = model.DepId });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            var spec = await (from d in DBContext.Specialities where d.Id.ToString() == Id select d).FirstOrDefaultAsync();
            if (spec != null)
            {
                var dep = await (from i in DBContext.Departments.Include(d => d.Specialities)
                                  where i.Specialities.Contains(spec)
                                  select i).FirstOrDefaultAsync();
                DBContext.Remove(spec);
                await DBContext.SaveChangesAsync();
                DisciplineFilesChecker checker = new DisciplineFilesChecker();
                checker.Check(environment, DBContext);
                return RedirectToAction("Index", new { DepId = dep.Id });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}