using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.ViewModels;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Data.OleDb;
using System.Data;

namespace IOS_PROJECT3.Controllers
{
    public class UsersAdminController : Controller
    {

        UserManager<EUser> userManager;
        DBMergedContext database;
        IWebHostEnvironment enviroment;
        
        public UsersAdminController(UserManager<EUser> userManager, DBMergedContext db, IWebHostEnvironment enviroment)
        {
            this.userManager = userManager;
            database = db;
            this.enviroment = enviroment;
           
        }

        public IActionResult Index()
        {
            var model = new UsersAdminViewModel(database)
            {
                Users = userManager.Users.ToList(),
                WhyTitles = new Dictionary<EUser, string>()
            };
            return View(model);
        }



        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MassRegistration(MassRegViewModel model)
        {
            for(int i=0;i<model.Count;i++)
            {
                var fio = model.FIOs[i];
                var email = model.Emails[i];
                var pass = model.Passwords[i];
                var role = model.Roles[i];
                if(!String.IsNullOrWhiteSpace(fio)&&
                    !String.IsNullOrWhiteSpace(email)&&
                    !String.IsNullOrWhiteSpace(pass)&&
                    !String.IsNullOrWhiteSpace(role))
                {
                    try
                    {
                        EUser user = new EUser()
                        {
                            Email = email,
                            UserName = email,
                            FIO = fio
                        };
                        var result = await userManager.CreateAsync(user, pass);
                        var result2 = await userManager.AddToRoleAsync(user, role);
                        if (!result.Succeeded||!result2.Succeeded)
                            throw new Exception("UserManager error, please verify data for user " + email);
                    }
                    catch(Exception e)
                    {
                        //доделать catch
                    }
                }//создать лист неудачников, показать его на отдельной или текущей вьюхе
            }
            return RedirectToAction("Index");

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                EUser user = new EUser { Email = model.Email, UserName = model.Email, FIO = model.FIO };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            EUser user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            EditUserViewModel model = new EditUserViewModel { Id = user.Id, Email = user.Email, FIO = user.FIO };
            return View(model);
        }
        public async Task<IActionResult> ResetPassword(string Id)
        {
            EUser user = await userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound();
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, "Default@Pass123");
            if (result.Succeeded)
            {
                await userManager.ResetAccessFailedCountAsync(user);
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View();

        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                EUser user = await userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.FIO = model.FIO;//потом можно разделить
                    var result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                    }
                }
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            EUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                try
                {
                    IdentityResult result = await userManager.DeleteAsync(user);
                }
                catch (Exception ex)
                {
                    // DeleteErrorMessage = "Ошибка блять ебаный тебя рот долбоеб тупой блять";

                    return RedirectToAction("Index");
                }

            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile uploadedFile)
        {
            if (uploadedFile != null)
            {
                // путь к папке Files
                string path = "/RegFiles/" + uploadedFile.FileName;
                if (!path.EndsWith(".xlsx"))
                    throw new Exception("File is not Excel file");
                // сохраняем файл в папку Files в каталоге wwwroot
                using (var fileStream = new FileStream(enviroment.WebRootPath + path, FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fileStream);
                }
               // RegFile filereg = new RegFile { Name = uploadedFile.FileName, Path = enviroment.WebRootPath + path };
                return RedirectToAction("MassRegistration",new {Path= (enviroment.WebRootPath + path)});
            }
            return RedirectToAction("Index");
            
        }

        public List<string> ReadColumn(string path, int column)
        {
            OleDbConnection conn = new OleDbConnection();
            //conn.ConnectionString = conn.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +path
            // +";Extended Properties='Excel 12.0 Xml;HDR=NO;IMEX=1;MAXSCANROWS=0'";
            conn.ConnectionString = String.Format(@"Provider=Microsoft.ACE.OLEDB.12.0;Excel 12.0 Xml;HDR=No;Data Source={0}", path);
            var comm = new OleDbCommand();
            comm.Connection = conn;
            comm.CommandText = "Select * from [Лист1$]";
            OleDbDataAdapter adapter = new OleDbDataAdapter(comm.CommandText, conn);
            DataTable dsXLS = new DataTable();
            adapter.Fill(dsXLS);
            List<string> result = new List<string>();

            var foo = dsXLS.Rows;
            for (int i = 0; i < foo.Count; i++)
            {
                result.Add(foo[i].ItemArray[column].ToString());
            }
            return result;
        }
        public IActionResult MassRegistration(string Path)
        {
            MassRegViewModel model = new MassRegViewModel()
            {
                FIOs = ReadColumn(Path, 0),
                Emails = ReadColumn(Path, 1),
                Passwords = ReadColumn(Path, 2),
                Roles = ReadColumn(Path, 3)
            };
            int em = model.Emails.Count;
            int fi = model.FIOs.Count;
            int pa = model.FIOs.Count;
            int ro = model.Roles.Count;
            model.Count = Math.Max(Math.Max(em, fi), Math.Max(pa, ro));
            return View(model);                       
        }
    }
}
