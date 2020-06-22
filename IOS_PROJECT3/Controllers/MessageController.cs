using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.ViewModels;
using IOS_PROJECT3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace IOS_PROJECT3.Controllers
{
    public class MessageController : Controller
    {
        DBMergedContext DBContext;
        UserManager<EUser> UserManager;
        public MessageController(DBMergedContext context, UserManager<EUser> userManager)
        {
            DBContext = context;
            UserManager = userManager;
        }
        public  IActionResult Index(string ContainerType, string ContainerId, string SenderEmail)
        {
            var cid = ContainerId;
            var typ = ContainerType;
            var sem = SenderEmail;
            var model = new MessageViewModel()
            {
                
                ContainerType=typ,
                SenderEmail=sem.ToLower()
            };
            if (typ != "All")
                model.ContainerId = cid;

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(MessageViewModel model)
        {
            try
            {
                //очень много вариков выборки получателей
                model.Receivers = new List<EUser>();
                if (model.ContainerType == "Institution")
                {
                    var inst = await (from i in DBContext.Institutions.Include(s => s.Departments).Include(m => m.Manager)
                                      where i.Id.ToString() == model.ContainerId
                                      select i).FirstOrDefaultAsync();
                    var deps = await (from d in DBContext.Departments.Include(h => h.HeadTeacher).Include(s=>s.Specialities)
                                      where inst.Departments.Contains(d)
                                      select d).ToListAsync();
                    if (model.SendToStaff)
                    {
                        if (inst.Manager.Email.ToLower() != model.SenderEmail)
                            model.Receivers.Add(inst.Manager);
                    }
                    foreach (var dep in deps)
                    {
                        var specs = await (from s in DBContext.Specialities.Include(s => s.Students).Include(di => di.Disciplines)
                                           where dep.Specialities.Contains(s)
                                           select s).ToListAsync();
                        if (model.SendToStaff)
                        {
                            if (model.SenderEmail != dep.HeadTeacher.Email.ToLower())
                                model.Receivers.Add(dep.HeadTeacher);
                        }
                        foreach (var spec in specs)
                        {
                            if(model.SendToStudents)
                            {
                                foreach (var stud in spec.Students)
                                {
                                    model.Receivers.Add(stud);
                                }
                            }
                            
                            if (model.SendToStaff)
                            {
                                var discs = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                                   where spec.Disciplines.Contains(di)
                                                   select di).ToListAsync();
                                foreach (var di in discs)
                                {
                                    if (di.Teacher.Email.ToLower() != model.SenderEmail)
                                        model.Receivers.Add(di.Teacher);
                                }
                            }
                        }
                    }

                }
                else if (model.ContainerType == "Department")
                {
                    var dep = await (from d in DBContext.Departments.Include(s => s.Specialities).Include(h => h.HeadTeacher)
                                     where d.Id.ToString() == model.ContainerId
                                     select d).FirstOrDefaultAsync();
                    if (model.SendToStaff)
                    {
                        if (model.SenderEmail != dep.HeadTeacher.Email.ToLower())
                            model.Receivers.Add(dep.HeadTeacher);
                    }

                    var specs = await (from s in DBContext.Specialities.Include(s => s.Students).Include(di => di.Disciplines)
                                       where dep.Specialities.Contains(s)
                                       select s).ToListAsync();
                    foreach (var spec in specs)
                    {
                        if(model.SendToStudents)
                        {
                            foreach (var stud in spec.Students)
                            {
                                model.Receivers.Add(stud);
                            }
                        }
                        
                        if (model.SendToStaff)
                        {
                            var discs = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                               where spec.Disciplines.Contains(di)
                                               select di).ToListAsync();
                            foreach (var di in discs)
                            {
                                if (di.Teacher.Email.ToLower() != model.SenderEmail)
                                    model.Receivers.Add(di.Teacher);
                            }
                        }
                    }

                }
                else if (model.ContainerType == "Speciality")
                {
                    var spec = await (from s in DBContext.Specialities.Include(s => s.Students).Include(di => di.Disciplines)
                                      where s.Id.ToString() == model.ContainerId
                                      select s).FirstOrDefaultAsync();
                    if(model.SendToStudents)
                    {
                        foreach (var stud in spec.Students)
                        {
                            model.Receivers.Add(stud);
                        }
                    }
                    if (model.SendToStaff)
                    {
                        var discs = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                           where spec.Disciplines.Contains(di)
                                           select di).ToListAsync();
                        foreach (var di in discs)
                        {
                            if (di.Teacher.Email.ToLower() != model.SenderEmail)
                                model.Receivers.Add(di.Teacher);
                        }
                    }
                }
                else if (model.ContainerType == "Discipline")
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id.ToString() == model.ContainerId
                                      select di).FirstOrDefaultAsync();
                    var spec = await (from s in DBContext.Specialities.Include(d => d.Disciplines).Include(s => s.Students)
                                      where s.Disciplines.Contains(disc)
                                      select s).FirstOrDefaultAsync();
                    if(model.SendToStaff)
                    {
                        if (disc.Teacher.Email.ToLower() != model.SenderEmail)
                            model.Receivers.Add(disc.Teacher);
                    }
                    if(model.SendToStudents)
                    {
                        foreach (var stud in spec.Students)
                        {
                            model.Receivers.Add(stud);
                        }
                    }
                    
                }
                else if (model.ContainerType == "All")
                {
                    if (model.SendToStaff&&model.SendToStudents)
                    {
                        var users = await UserManager.Users.ToListAsync();
                        foreach (var u in users)
                        {
                            if (u.Email.ToLower() != model.SenderEmail)
                                model.Receivers.Add(u);
                        }
                    }
                    else if(model.SendToStudents&&!model.SendToStaff)
                    {
                        var users = await UserManager.GetUsersInRoleAsync("Student");
                        foreach (var u in users)
                        {
                            model.Receivers.Add(u);
                        }
                    }
                    else if(model.SendToStaff)
                    {
                        var teachers= await UserManager.GetUsersInRoleAsync("Teacher");
                        var managers = await UserManager.GetUsersInRoleAsync("Manager");
                        var admins = await UserManager.GetUsersInRoleAsync("Admin");
                        List<EUser> union = new List<EUser>(teachers);
                        union.AddRange(managers);
                        union.AddRange(admins);
                        foreach (var u in union)
                        {
                            if (u.Email.ToLower() != model.SenderEmail)
                                model.Receivers.Add(u);
                        }
                    }

                }
                if (model.Receivers.Count > 0)
                {
                    var ES = new EmailService();
                    List<string> Remails = new List<string>();
                    foreach (var r in model.Receivers)
                        Remails.Add(r.Email);
                    model.Message += "<br>" +
                        "<b><Отправлено из системы ИОС СГТУ. Не отвечайте на это сообщение, ответ не дойдет до отправителя></b>";
                    await ES.SendEmailAsync(Remails.ToArray(), model.Subject, model.Message);
                    Remails.Clear();
                    foreach (var r in model.Receivers)
                        Remails.Add(r.Email + " | " + r.FIO);
                    return RedirectToAction("Success", new { Receivers = Remails });
                }
            }
            catch(Exception e)
            {
                return RedirectToAction("Fail", new { Reason = "Пустое письмо не может быть отправлено\n"+e.Message});//e.Message });
            }
            return RedirectToAction("Fail", new{ Reason="Нет ни одного получателя" });
        }

        public IActionResult Success(List<string> Receivers)
        {
            return View(Receivers);
        }
        public IActionResult Fail(string Reason)
        {
            ViewBag.Reason = Reason;
            return View();
        }
    }
}