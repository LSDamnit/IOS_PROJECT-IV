using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private DBMergedContext DBContext;
        private RoleManager<IdentityRole> roleManager;
        private UserManager<EUser> userManager;

        public RolesController(DBMergedContext DBContext, RoleManager<IdentityRole> roleManager, UserManager<EUser> userManager)
        {
            this.DBContext = DBContext;
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [Authorize(Grants.Grants.Roles.View)]
        public IActionResult Index()
        {
            var model = new RolesViewModel(DBContext)
            {
                allRoles = roleManager.Roles.ToList()
            };
            return View(model);
        }

        //[Authorize(Grants.Grants.Roles.Create)]
        public IActionResult Create()
        {
            return View();
        }

        //[Authorize(Grants.Grants.Roles.Create)]
        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!String.IsNullOrWhiteSpace(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    // return View(roleName);//возможно стоит изменить на редирект
                    return RedirectToAction("Index");
                }
                else
                {
                    var Errors = result.Errors;
                    foreach (var err in Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                    }
                }
            }
            return View(roleName);
        }

        //[Authorize(Grants.Grants.Roles.EditRole)]
        public async Task<IActionResult> EditRole(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            var model = new EditRoleViewModel()
            {
                roleId = role.Id,
                allGrants = DBContext.Grants.ToList(),
                roleGrantsId = DBContext.RolesToGrants.Where(rtg => rtg.RoleId == roleId).Select(rtg => rtg.GrantId).ToList(),
                Name = role.Name
            };
            //var grants = DBContext.Grants.Where(g => roleGrants.Contains(g.Id)).ToList();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.roleId);
            if (role != null)
            {
                role.Name = model.Name;
                DBContext.Update(role).Entity.Name = model.Name;

                var deleteGrants = DBContext.RolesToGrants.Where(rtg => rtg.RoleId == role.Id);
                DBContext.RolesToGrants.RemoveRange(deleteGrants);
                await DBContext.SaveChangesAsync();
                if (model.roleGrantsId != null)
                {
                    foreach (var grant in model.roleGrantsId)
                    {
                        if ((await DBContext.RolesToGrants.Where(rtg => rtg.RoleId == role.Id && rtg.GrantId == grant).FirstOrDefaultAsync()) == null)
                        {
                            DBContext.RolesToGrants.Add(new ERolesToGrants()
                            {
                                RoleId = role.Id,
                                GrantId = grant
                            });
                        }
                    }
                }

                await DBContext.SaveChangesAsync();

                return RedirectToAction("Index", "Roles");
            }

            return NotFound();
        }

        //[Authorize(Grants.Grants.Roles.Delete)]
        [HttpPost]
        public async Task<IActionResult> Delete(string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if(role!=null)
            await roleManager.DeleteAsync(role);
            
            return RedirectToAction("Index");
        }

        public IActionResult UserList()
        {
            return View(userManager.Users.ToList());
        }

        //[Authorize(Grants.Grants.Roles.Edit)]
        public async Task<IActionResult> Edit(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if(user!=null)
            {
                var userRoles = await userManager.GetRolesAsync(user);
                var allRoles = roleManager.Roles.ToList();
                var viewModel = new ChangeRoleViewModel
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    AllRoles = allRoles,
                    UserRoles = userRoles
                };
                return View(viewModel);
            }
            return NotFound();
        }

        //[Authorize(Grants.Grants.Roles.Edit)]
        [HttpPost]
        public async Task<IActionResult> Edit(string userId, List<string> roles)
        {
            
            EUser user = await userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var userRoles = await userManager.GetRolesAsync(user);              
                var allRoles = roleManager.Roles.ToList();
                var addedRoles = roles.Except(userRoles);
                var removedRoles = userRoles.Except(roles);

                await userManager.AddToRolesAsync(user, addedRoles);

                await userManager.RemoveFromRolesAsync(user, removedRoles);

                return RedirectToAction("Index", "UsersAdmin");
            }

            return NotFound();
        }
    }
}