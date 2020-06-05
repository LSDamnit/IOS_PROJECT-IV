using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IOS_PROJECT3.Controllers
{
    public class DisciplinesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}