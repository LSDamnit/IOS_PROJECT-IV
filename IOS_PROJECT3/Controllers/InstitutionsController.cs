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
    public class InstitutionsController : Controller
    {
        private DBMergedContext DBContext;
        private UserManager<EUser> UserManager;
        IWebHostEnvironment environment;
        GrantCheckService checkService;
        public InstitutionsController(GrantCheckService checkService, DBMergedContext context, UserManager<EUser> manager, IWebHostEnvironment environment)
        {
            this.checkService = checkService;
            DBContext = context;
            UserManager = manager;
            this.environment = environment;
        }

        [Authorize(Grants.Grants.Institutions.View)]
        public async Task<IActionResult> Index()
        {
            
            InstitutionsViewModel model = new InstitutionsViewModel()
            {
                Institutions = DBContext.Institutions.Include(m=>m.Manager).ToList(),
                userGrants = await checkService.getUserGrants(User)
            };
            
            return View(model);
        }

        [Authorize(Grants.Grants.Institutions.Edit)]
        public async Task<IActionResult> Edit(string Id)
        {
            EditInstitutionViewModel model = new EditInstitutionViewModel()
            {
                AvailableManagers = await UserManager.GetUsersInRoleAsync("Manager"),
                Name = (from inst in DBContext.Institutions where inst.Id.ToString() == Id select inst.Name).FirstOrDefaultAsync().Result,
                ManagerId = (from inst in DBContext.Institutions.Include(m => m.Manager)
                             where inst.Id.ToString() == Id
                             select inst.Manager.Id).FirstOrDefaultAsync().Result,
                Idinst=Id,
                userGrants = await checkService.getUserGrants(User)
                // Institution= (from inst in DBContext.Institutions.Include(m => m.Manager) where inst.Id.ToString() == Id select inst).FirstOrDefault()

            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Grants.Grants.Institutions.Edit)]
        public async Task<IActionResult> Edit(EditInstitutionViewModel model)
        {
            if (ModelState.IsValid) 
            { 
            var manager = await UserManager.FindByIdAsync(model.ManagerId);
            string name = model.Name;
            
            
                         
                var inst = (from i in DBContext.Institutions.Include(m => m.Manager)
                            where i.Id.ToString() == model.Idinst
                            select i).FirstOrDefaultAsync().Result;

                DBContext.Update(inst).Entity.Manager = manager;
                DBContext.Update(inst).Entity.Name = name;

                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
                model.AvailableManagers = await UserManager.GetUsersInRoleAsync("Manager");
            return View(model);
        }

        [Authorize(Grants.Grants.Institutions.Create)]
        public async Task<IActionResult> Create()
        {
            CreateInstitutionViewModel model = new CreateInstitutionViewModel()
            {
                AvailableManagers = await UserManager.GetUsersInRoleAsync("Manager"),
                userGrants = await checkService.getUserGrants(User)
            };
            return View(model);
        }

        [HttpPost]
        [Authorize(Grants.Grants.Institutions.Create)]
        public async Task<IActionResult> Create(CreateInstitutionViewModel model)
        {
            if (ModelState.IsValid)
            {
                var manager = await UserManager.FindByIdAsync(model.ManagerId);
                string name = model.Name;


                EInstitution inst = new EInstitution()
                {
                    Name = name,
                    Manager = manager

                };
                DBContext.Institutions.Add(inst);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
                model.AvailableManagers = await UserManager.GetUsersInRoleAsync("Manager");
            return View(model);
        }

        [HttpPost]
        [Authorize(Grants.Grants.Institutions.Delete)]
        public async Task<IActionResult> Delete(string Id)
        {
            var inst = (from i in DBContext.Institutions
                        where i.Id.ToString() == Id
                        select i).FirstOrDefaultAsync().Result;
            DBContext.Remove(inst);
           // DBContext.Institutions.Find(inst).En
            await DBContext.SaveChangesAsync();
            DisciplineFilesChecker checker = new DisciplineFilesChecker();
            checker.Check(environment, DBContext);
            return RedirectToAction("Index");
        }
    }
}