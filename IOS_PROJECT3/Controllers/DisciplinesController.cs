using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

namespace IOS_PROJECT3.Controllers
{
    public class DisciplinesController : Controller
    {
        private DBMergedContext DBContext;
        UserManager<EUser> userManager;
        IWebHostEnvironment enviroment;
        public DisciplinesController(DBMergedContext context, UserManager<EUser> userManager, IWebHostEnvironment enviroment)
        {
            DBContext = context;
            this.userManager = userManager;
            this.enviroment = enviroment;
        }
        public async Task<IActionResult> Index(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines).Include(s => s.Students)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
            if (spec != null)
            {
                var dis = await (from d in DBContext.Disciplines.Include(t => t.Teacher)
                                 where spec.Disciplines.Contains(d)
                                 select d).ToListAsync();
                var dep = await (from i in DBContext.Departments.Include(d => d.Specialities)
                                 where i.Specialities.Contains(spec)
                                 select i).FirstOrDefaultAsync();
                var inst = await (from i in DBContext.Institutions.Include(d => d.Departments).Include(m => m.Manager)
                                  where i.Departments.Contains(dep)
                                  select i).FirstOrDefaultAsync();

                var model = new DisciplinesViewModel()
                {
                    SpecialityId = spec.Id.ToString(),
                    SpecialityName = spec.Name,
                    InstId = inst.Id.ToString(),
                    InstManagerId = inst.Manager.Id,
                    InstManagerEmail = inst.Manager.Email,
                    Disciplines = dis,
                    Students = spec.Students
                };
                return View(model);

            }
            return View();//сделать редирект на страницу с ошибкой
        }

        public async Task<IActionResult> Create(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
            if (spec != null)
            {
                var model = new CreateDisciplineViewModel()
                {
                    SpecialityId = spec.Id.ToString(),
                    AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher")
                };
                return View(model);
            }
            ModelState.AddModelError("Create", "No such speciality");
            return RedirectToAction("Index");//untested
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDisciplineViewModel model)
        {
            var teacher = await userManager.FindByIdAsync(model.TeacherId);
            var spec = await (from s in DBContext.Specialities.Include(d => d.Disciplines)
                              where s.Id.ToString() == model.SpecialityId
                              select s).FirstOrDefaultAsync();
            var name = model.Name;
            var info = "";
            if (!String.IsNullOrWhiteSpace(model.Info))
                info = model.Info;
            if (!String.IsNullOrWhiteSpace(name) && teacher != null && !String.IsNullOrWhiteSpace(model.ExamType))
            {
                EDiscipline disc = new EDiscipline()
                {
                    Name = name,
                    LectionH = model.LectionH,
                    PracticeH = model.PracticeH,
                    ExamType = model.ExamType,
                    Teacher = teacher,
                    About = info

                };
                DBContext.Disciplines.Add(disc);
                spec.Disciplines.Add(disc);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { SpecId = spec.Id });
            }
            ModelState.AddModelError("Create", "Error in disc create");
            return View(model);
        }

        public async Task<IActionResult> Edit(string DiscId)
        {
            var disc = await (from d in DBContext.Disciplines.Include(h => h.Teacher) where d.Id.ToString() == DiscId select d).FirstOrDefaultAsync();
            if (disc != null)
            {
                var spec = await (from i in DBContext.Specialities.Include(d => d.Disciplines)
                                  where i.Disciplines.Contains(disc)
                                  select i).FirstOrDefaultAsync();
                var model = new EditDisciplineViewModel()
                {
                    Name = disc.Name,
                    DisciplineId = disc.Id.ToString(),
                    SpecialityId = spec.Id.ToString(),
                    LectionH = disc.LectionH,
                    PracticeH = disc.PracticeH,
                    ExamType = disc.ExamType,
                    TeacherId = disc.Teacher.Id,
                    Info=disc.About,
                    AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher")

                };
                return View(model);
            }
            return View();//сделать редирект на ошибку
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditDisciplineViewModel model)
        {
            var teacher = await userManager.FindByIdAsync(model.TeacherId);
            string name = model.Name;
            var info = "";
            if (!String.IsNullOrWhiteSpace(model.Info))
                info = model.Info;
            if (teacher != null && !String.IsNullOrWhiteSpace(name)&& !String.IsNullOrWhiteSpace(model.ExamType))
            {
                var disc = await (from d in DBContext.Disciplines.Include(h => h.Teacher)
                                 where d.Id.ToString() == model.DisciplineId
                                 select d).FirstOrDefaultAsync();

                DBContext.Update(disc).Entity.Teacher = teacher;
                DBContext.Update(disc).Entity.Name = name;
                DBContext.Update(disc).Entity.ExamType = model.ExamType;
                DBContext.Update(disc).Entity.LectionH = model.LectionH;
                DBContext.Update(disc).Entity.PracticeH = model.PracticeH;
                DBContext.Update(disc).Entity.About = info;
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { SpecId = model.SpecialityId });
            }
            ModelState.AddModelError("Edit", "Error in department edit");
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(string Id)
        {
            var disc = await (from d in DBContext.Disciplines where d.Id.ToString() == Id select d).FirstOrDefaultAsync();
            if (disc != null)
            {
                var spec = await (from i in DBContext.Specialities.Include(d => d.Disciplines)
                                  where i.Disciplines.Contains(disc)
                                  select i).FirstOrDefaultAsync();
                DBContext.Remove(disc);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("Index", new { SpecId = spec.Id });
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> DeleteStudent(string Id)
        {
            var user = await (from u in DBContext.Users
                              where u.Id == Id
                              select u).FirstOrDefaultAsync();

            var spec = await (from s in DBContext.Specialities.Include(s => s.Students)
                                      where s.Students.Contains(user)
                                      select s).FirstOrDefaultAsync();
                DBContext.Update(spec).Entity.Students.Remove(user);
                await DBContext.SaveChangesAsync();

            return RedirectToAction("Index", new { SpecId = spec.Id });
        }
        public async Task<IActionResult> AddStudent(string SpecId)
        {
            AddStudentViewModel model = new AddStudentViewModel()
            {
                AvailableStudents = await userManager.GetUsersInRoleAsync("Student"),
                context=DBContext,
                TargetSpecId=SpecId
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddStudentFillData(AddStudentViewModel model)
        {
            model.context = DBContext;
            await model.FillDataAsync();
            var user = await (from u in DBContext.Users where u.NormalizedEmail == model.Email.ToUpper() select u).FirstOrDefaultAsync();
            if (user==null||!(await userManager.IsInRoleAsync(user, "Student")))
            {
                ModelState.AddModelError("Not student", "User isn't in role 'Student'");
                return RedirectToAction("AddStudent", new { SpecId = model.TargetSpecId });
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddStudent(AddStudentViewModel model)
        {
            var student = await (from s in DBContext.Users
                                 where s.Email.ToLower() == model.Email.ToLower()
                                 select s).FirstOrDefaultAsync();
            var specTarget = await (from s in DBContext.Specialities.Include(s => s.Students)
                                    where s.Id.ToString() == model.TargetSpecId
                                    select s).FirstOrDefaultAsync();
            var specCurrent = await (from s in DBContext.Specialities.Include(s => s.Students)
                                     where s.Id.ToString() == model.CurrentSpecId
                                     select s).FirstOrDefaultAsync();
            if(student!=null&&specTarget!=null)
            {
                DBContext.Update(specTarget).Entity.Students.Add(student);
                if (specCurrent != null)
                    DBContext.Update(specCurrent).Entity.Students.Remove(student);
                await DBContext.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { SpecId = model.TargetSpecId });
        }
           
        public async Task<IActionResult> MassRegistration(string Path, string Id)
        {
            try
            {
                ExcelParser ep = new ExcelParser();
                MassRegViewModel model = new MassRegViewModel()
                {
                    FIOs = ep.ReadColumn(Path, 0),
                    Emails = ep.ReadColumn(Path, 0),
                   // Passwords = ep.ReadColumn(Path, 2),
                    //Roles = ep.ReadColumn(Path, 3)
                };
                model.TargetSpecId = Id;
                model.Count = model.Emails.Count;
                for (int i=0;i<model.Count;i++)
                {
                    var user = await userManager.FindByNameAsync(model.Emails[i]);
                    if (user != null)
                        model.FIOs[i] = user.FIO;
                    else model.FIOs[i] = "Пользователь не найден";
                }
                System.IO.File.Delete(Path);
                return View(model);
            }
            catch (Exception e)
            {
                return RedirectToAction("ErrorLoadingFile", "Errors", new { message = e.Message });
            }

        }
        [HttpPost]
        public async Task<IActionResult> MassRegistration(MassRegViewModel model)
        {
            List<string> FailedUsers = new List<string>();

            for (int i = 0; i < model.Count; i++)
            {
                var email = model.Emails[i];
                string fio = "";
                bool isOk = true;
                string reason = "";
                if (String.IsNullOrWhiteSpace(email))
                {
                    isOk = false;
                    reason = "В Email было передано пустое значение";
                }
                if (isOk)
                {
                    var spec = await (from s in DBContext.Specialities.Include(s => s.Students)
                                      where s.Id.ToString() == model.TargetSpecId select s).FirstOrDefaultAsync();
                    var user = await (from u in DBContext.Users
                                      where u.Email.ToLower() == email.ToLower() select u).FirstOrDefaultAsync();
                    
                    if(user==null)
                    {
                        isOk = false;
                        reason = "Пользователь не найден";
                    }
                    if (isOk)
                    {
                        fio = user.FIO;
                        if (!(await userManager.IsInRoleAsync(user, "Student")))
                        {
                            isOk = false;
                            reason = "Пользователь не принадлежит к роли 'Student'";
                        }
                        if (isOk)
                        {  
                            var oldspec = await (from s in DBContext.Specialities.Include(s => s.Students)
                                                 where s.Students.Contains(user)
                                                 select s).FirstOrDefaultAsync();
                            DBContext.Update(spec).Entity.Students.Add(user);
                            if (oldspec != null)
                                DBContext.Update(oldspec).Entity.Students.Remove(user);

                        }
                    }

                }
                if (!isOk)
                {
                    if (String.IsNullOrWhiteSpace(fio))
                        fio = "(ФИО не определено)";
                    if (String.IsNullOrWhiteSpace(email))
                        email = "(Пустой Email)";
                        FailedUsers.Add(email + " | " + fio + " - " + reason);
                }

            }
            await DBContext.SaveChangesAsync();
            if (FailedUsers.Count == 0)
                return RedirectToAction("Index", new { SpecId = model.TargetSpecId });
            else return RedirectToAction("MassRegistrationFails", new { list = FailedUsers, Id=model.TargetSpecId });
        }
        public IActionResult MassRegistrationFails(List<string> list, string Id)
        {
            ViewBag.SpecId = Id;
            var model = new MassRegErrorViewModel()
            {
                FailedUsers = list
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AddFile(IFormFile uploadedFile, string SpecId)
        {
            try
            {
                if (uploadedFile != null)
                {
                    // путь к папке Files
                    string path = "/RegToSpecialityFiles/" + uploadedFile.FileName;
                    if (!path.EndsWith(".xlsx"))
                        throw new Exception("File is not .xlsx file");

                    using (var fileStream = new FileStream(enviroment.WebRootPath + path, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }
                    // RegFile filereg = new RegFile { Name = uploadedFile.FileName, Path = enviroment.WebRootPath + path };
                    return RedirectToAction("MassRegistration", new { Path = (enviroment.WebRootPath + path), Id=SpecId });
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("ErrorLoadingFile", "Errors", new { message = e.Message });
            }
            return RedirectToAction("Index");

        }
    }
}