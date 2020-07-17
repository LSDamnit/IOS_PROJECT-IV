using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Grants;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace IOS_PROJECT3.Controllers
{
    public class PersonalPageController : Controller
    {
        private readonly DBMergedContext DBContext;
        GrantCheckService checkService;
       public PersonalPageController(DBMergedContext context, GrantCheckService checkService)
        {
            this.DBContext = context;
            this.checkService = checkService;
        }
        public async Task<IActionResult> Index(List<string> SecuritySettingsErrors,bool SettingsChanged)
        {
            if (!User.Identity.IsAuthenticated)
            {
                UserPageViewModel model = new UserPageViewModel(DBContext)
                {
                    userGrants = await checkService.getUserGrants(User)
                };

                return View(model);
            }
            else
            {
                UserPageViewModel model = new UserPageViewModel(DBContext)
                {
                    UserFIO = (from usr in DBContext.Users
                               where usr.Email == User.Identity.Name
                               select usr.FIO).FirstOrDefault(),
                    UserId = (from usr in DBContext.Users
                              where usr.Email == User.Identity.Name
                              select usr.Id).FirstOrDefault(),
                    userGrants = await checkService.getUserGrants(User)
                };
                if(SecuritySettingsErrors!=null)
                {
                    ViewBag.Errors = new List<string>();
                    foreach(var err in SecuritySettingsErrors)
                    {
                        ViewBag.Errors.Add(err);
                    }
                }
                if(SettingsChanged)
                {
                    ViewBag.Success = "Настройки безопасности успешно обновлены";
                }
                await model.CheckAsync();
                return View(model);
            }
        }
    }
}