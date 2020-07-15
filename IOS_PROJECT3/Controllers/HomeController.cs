using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.Grants;
using IOS_PROJECT3.ViewModels;
namespace IOS_PROJECT3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBMergedContext DBContext;
        GrantCheckService checkService;
        //  private UserPageViewModel ViewModel;
        public HomeController(GrantCheckService checkService, ILogger<HomeController> logger, DBMergedContext dbContext)
        {
            DBContext = dbContext;
            this.checkService = checkService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if(!User.Identity.IsAuthenticated)
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
                await model.CheckAblesAsync();
                return View(model);
            }
            
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
