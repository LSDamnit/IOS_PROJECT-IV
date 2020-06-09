using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace IOS_PROJECT3.Controllers
{
    public class ErrorsController : Controller
    {
        public IActionResult ErrorLoadingFile(string message)
        {
            ViewBag.Message = message;
            return View();
        }
    }
}