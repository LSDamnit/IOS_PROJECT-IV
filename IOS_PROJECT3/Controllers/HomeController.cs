using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
namespace IOS_PROJECT3.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DBMergedContext DBContext;
      //  private UserPageViewModel ViewModel;
        public HomeController(ILogger<HomeController> logger, DBMergedContext dbContext)
        {
            DBContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if(!User.Identity.IsAuthenticated)
            {
                UserPageViewModel model = new UserPageViewModel(DBContext);
                
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
                               select usr.Id).FirstOrDefault()
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
