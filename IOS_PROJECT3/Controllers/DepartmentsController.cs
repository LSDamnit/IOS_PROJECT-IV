﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using IOS_PROJECT3.Grants;



namespace IOS_PROJECT3.Controllers
{
    public class DepartmentsController : Controller
    {
        private DBMergedContext DBContext;
        private UserManager<EUser> UserManager;
        IWebHostEnvironment environment;
        GrantCheckService checkService;
        public DepartmentsController(GrantCheckService checkService, DBMergedContext context, UserManager<EUser> manager, IWebHostEnvironment environment)
        {
            this.checkService = checkService;
            DBContext = context;
            UserManager = manager;
            this.environment = environment;
        }
        public async Task<IActionResult> Index(string InstId,bool FromPP)
        {
            //-----!!!! Это был тест отправки сообщений - пусть он тут побудет
           // EmailService es = new EmailService();
           // await es.SendEmailAsync("yuridemydko@gmail.com", "SMTP_TEST", "YOLO");
            //-----!!!!!
            var inst = await (from i in DBContext.Institutions.Include(d => d.Departments).Include(m => m.Manager)
                              where i.Id.ToString() == InstId
                              select i).FirstOrDefaultAsync();
            var deps = await (from d in DBContext.Departments.Include(h => h.HeadTeacher)
                              where inst.Departments.Contains(d)
                              select d).ToListAsync();
            
                var departs = deps;
                var man = inst.Manager;
                var name = inst.Name;
                var mail = inst.Manager.Email;
                //var manager
                var model = new DepartmentsViewModel()
                {
                    InstitutionId = InstId,
                    InstitutionName = name,
                    ManagerId = man.Id,
                    Departments = deps,
                    ManagerEmail=mail,
                    userGrants = await checkService.getUserGrants(User)
                };
            model.FromPP = FromPP;
                return View(model);
            
            
        }

        public async Task<IActionResult> Create(string InstId)
        {
            var id = InstId;
            CreateDepartmentViewModel model = new CreateDepartmentViewModel()
            {
                AvailableTeachers = await UserManager.GetUsersInRoleAsync("Teacher"),
                InstId = id,
                userGrants = await checkService.getUserGrants(User)
            };
            return View(model);
        }
       
        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var headt = await UserManager.FindByIdAsync(model.HeadTeacherId);
                var name = model.Name;
                var inst = await (from i in DBContext.Institutions.Include(d => d.Departments) where i.Id.ToString() == model.InstId select i).FirstOrDefaultAsync();
                if (headt != null && !String.IsNullOrWhiteSpace(name))
                {
                    EDepartment dep = new EDepartment()
                    {
                        Name = name,
                        HeadTeacher = headt
                    };

                    inst.Departments.Add(dep);
                    DBContext.Departments.Add(dep);
                    await DBContext.SaveChangesAsync();

                    return RedirectToAction("Index", new { InstId = model.InstId });
                }
            }
            else
                model.AvailableTeachers = await UserManager.GetUsersInRoleAsync("Teacher");
            return View(model);
        }

        public async Task<IActionResult> Edit(string Id)
        {
           
            var dep = await (from d in DBContext.Departments.Include(h=>h.HeadTeacher) where d.Id.ToString() == Id select d).FirstOrDefaultAsync();
            if(dep!=null)
            {
                var inst = await (from i in DBContext.Institutions.Include(d => d.Departments) where
                              i.Departments.Contains(dep) select i).FirstOrDefaultAsync();
                var model = new EditDepartmentViewModel()
                {
                    Name = dep.Name,
                    DepartmentId = dep.Id.ToString(),
                    HeadTeacherId = dep.HeadTeacher.Id,
                    InstitutionId = inst.Id.ToString(),
                    InstitutionName = inst.Name,
                    AvailableTeachers=await UserManager.GetUsersInRoleAsync("Teacher"),
                    userGrants = await checkService.getUserGrants(User)

                };
                return View(model);
            }
            return View();//вряд-ли это произойдет, но я вообще хз, что тогда случится

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDepartmentViewModel model)
        {
            if (ModelState.IsValid) { 
            var teacher = await UserManager.FindByIdAsync(model.HeadTeacherId);
            string name = model.Name;

            
            
                var dep = await (from d in DBContext.Departments.Include(h => h.HeadTeacher)
                           where d.Id.ToString() == model.DepartmentId
                           select d).FirstOrDefaultAsync();

                DBContext.Update(dep).Entity.HeadTeacher = teacher;
                DBContext.Update(dep).Entity.Name = name;

                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { InstId = model.InstitutionId });
            }
            model.AvailableTeachers = await UserManager.GetUsersInRoleAsync("Teacher");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            var dep = await (from d in DBContext.Departments where d.Id.ToString() == Id select d).FirstOrDefaultAsync();
            if (dep != null)
            {
                var inst = await (from i in DBContext.Institutions.Include(d => d.Departments)
                                  where i.Departments.Contains(dep)
                                  select i).FirstOrDefaultAsync();
                DBContext.Remove(dep);
                await DBContext.SaveChangesAsync();
                DisciplineFilesChecker checker = new DisciplineFilesChecker();
                checker.Check(environment, DBContext);
                return RedirectToAction("Index", new { InstId = inst.Id });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}