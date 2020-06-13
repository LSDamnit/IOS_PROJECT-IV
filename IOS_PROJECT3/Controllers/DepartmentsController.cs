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
    public class DepartmentsController : Controller
    {
        private DBMergedContext DBContext;
        private UserManager<EUser> UserManager;
        public DepartmentsController(DBMergedContext context, UserManager<EUser> manager)
        {
            DBContext = context;
            UserManager = manager;
        }
        public async Task<IActionResult> Index(string InstId)
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
            if (inst != null)
            {
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
                    ManagerEmail=mail
                };
                return View(model);
            }
            else ModelState.AddModelError("Index", "No such institution");

            return View();
        }

        public async Task<IActionResult> Create(string InstId)
        {
            var id = InstId;
            CreateDepartmentViewModel model = new CreateDepartmentViewModel()
            {
                AvailableTeachers = await UserManager.GetUsersInRoleAsync("Teacher"),
                InstId = id
            };
            return View(model);
        }
       
        [HttpPost]
        public async Task<IActionResult> Create(CreateDepartmentViewModel model)
        {
            var headt = await UserManager.FindByIdAsync(model.HeadTeacherId);
            var name = model.Name;
            var inst = await (from i in DBContext.Institutions.Include(d=>d.Departments) where i.Id.ToString() == model.InstId select i).FirstOrDefaultAsync();
            if(headt!=null && !String.IsNullOrWhiteSpace(name))
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
            ModelState.AddModelError("Create", "Error in dep create");
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
                    AvailableTeachers=await UserManager.GetUsersInRoleAsync("Teacher")

                };
                return View(model);
            }
            return View();//вряд-ли это произойдет, но я вообще хз, что тогда случится

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDepartmentViewModel model)
        {
            var teacher = await UserManager.FindByIdAsync(model.HeadTeacherId);
            string name = model.Name;

            if (teacher != null && !String.IsNullOrWhiteSpace(name))
            {
                var dep = await (from d in DBContext.Departments.Include(h => h.HeadTeacher)
                           where d.Id.ToString() == model.DepartmentId
                           select d).FirstOrDefaultAsync();

                DBContext.Update(dep).Entity.HeadTeacher = teacher;
                DBContext.Update(dep).Entity.Name = name;

                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { InstId = model.InstitutionId });
            }
            ModelState.AddModelError("Edit", "Error in department edit");
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
                return RedirectToAction("Index", new { InstId = inst.Id });
            }
            return RedirectToAction("Index", "Home");
        }
    }
}