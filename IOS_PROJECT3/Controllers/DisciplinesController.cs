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
        IWebHostEnvironment environment;
        public DisciplinesController(DBMergedContext context, UserManager<EUser> userManager, IWebHostEnvironment environment)
        {
            DBContext = context;
            this.userManager = userManager;
            this.environment = environment;
        }
        public async Task<IActionResult> Index(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines).Include(s => s.Students).Include(s=>s.Schedules)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
            if (spec != null)
            {
                //var sched = await (from s in DBContext.WeekSchedules.Include(w => w.Schedule)
                  //                 where s.Speciality.Id == spec.Id
                    //               select s).FirstOrDefaultAsync();

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
                    Students = spec.Students,
                    Schedules = spec.Schedules.OrderBy(s => s.Name).ToList<EWeekSchedule>()
                };
                return View(model);

            }
            return View();//сделать редирект на страницу с ошибкой
        }
        public async Task<IActionResult> WeekSchedule(string WeekScheduleId)
        {
            var wsched = await (from s in DBContext.WeekSchedules.Include(s => s.Schedule)
                                where s.id.ToString() == WeekScheduleId
                                select s).FirstOrDefaultAsync();
            var dscheds = await (from s in DBContext.DaySchedules.Include(s => s.DisciplinesForDay)
                                 where s.WeekSchedule.id.ToString() == WeekScheduleId
                                 select s).ToListAsync();
            var model = new ScheduleViewModel()
            {
                WeekScheduleId = wsched.id.ToString(),
                WeekScheduleName = wsched.Name,
                ScheduleForDays=new Dictionary<string, List<EScheduleItem>>()
                {
                    {"mon",(from d in dscheds where d.DayNumber==0 select d.DisciplinesForDay).FirstOrDefault() },
                    {"tue",(from d in dscheds where d.DayNumber==1 select d.DisciplinesForDay).FirstOrDefault() },
                    {"wed",(from d in dscheds where d.DayNumber==2 select d.DisciplinesForDay).FirstOrDefault() },
                    {"thu",(from d in dscheds where d.DayNumber==3 select d.DisciplinesForDay).FirstOrDefault() },
                    {"fri",(from d in dscheds where d.DayNumber==4 select d.DisciplinesForDay).FirstOrDefault() },
                    {"sat",(from d in dscheds where d.DayNumber==5 select d.DisciplinesForDay).FirstOrDefault() },
                }
            };
            return View(model);
        }

        public async Task<IActionResult> CreateSchedule(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
            var discs = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                               where spec.Disciplines.Contains(di)
                               select di).ToListAsync();
            var model = new CreateScheduleViewModel()
            {
                SpecialityId = SpecId,
                AvailableDisciplines = discs
            };
            model.init();
            return View(model);
        }
        public async Task<IActionResult> EditSchedule(string WeekScheduleId)
        {
            var Sched = await (from s in DBContext.WeekSchedules.Include(s => s.Speciality).ThenInclude(di=>di.Disciplines)
                               .Include(s => s.Schedule)                               
                               where s.id.ToString()==WeekScheduleId select s).FirstOrDefaultAsync();
            var dScheds = await (from d in DBContext.DaySchedules.Include(d => d.DisciplinesForDay)
                                 where Sched.Schedule.Contains(d)
                                 select d).ToListAsync();
            Sched.Schedule = dScheds;
            var availablediscs = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                        where Sched.Speciality.Disciplines.Contains(di) select di).ToListAsync();
            var model = new EditScheduleViewModel()
            {
                AvailableDisciplines = availablediscs
            };
            model.init(Sched);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditSchedule(EditScheduleViewModel model)
        {
            var spec = await (from sp in DBContext.Specialities.Include(s => s.Schedules)
                              where sp.Id.ToString() == model.SpecialityId
                              select sp).FirstOrDefaultAsync();
            var oldShedule = await (from sc in DBContext.WeekSchedules
                                    where sc.id == model.WeekScheduleId
                                    select sc).FirstOrDefaultAsync();
            spec.Schedules.Remove(oldShedule);
           
            DBContext.Remove(oldShedule);
            if (model.WeekScheduleName == null)
                model.WeekScheduleName = "Без названия";

            var NewWeekSchedule = new EWeekSchedule()
            {
                Name = model.WeekScheduleName,
                Speciality = spec,
                Schedule = new List<EDaySchedule>(6)
            };
            // DBContext.WeekSchedules.Add(NewWeekSchedule);

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//mon
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 0,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.mon[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[0];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";

                NewWeekSchedule.Schedule[0].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//tue
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 1,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.tue[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[1];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[1].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//wed
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 2,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.wed[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[2];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[2].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//thu
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 3,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.thu[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[3];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[3].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//fri
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 4,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.fri[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[4];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[4].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//sat
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 5,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.sat[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[5];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[5].DisciplinesForDay.Add(discipline);
            }
            spec.Schedules.Add(NewWeekSchedule);            
            DBContext.Add(NewWeekSchedule);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("Index", new { SpecId = spec.Id });
        }
        [HttpPost]
        public async Task<IActionResult> CreateSchedule(CreateScheduleViewModel model)
        {
            var spec = await (from sp in DBContext.Specialities.Include(s=>s.Schedules)
                              where sp.Id.ToString() == model.SpecialityId select sp).FirstOrDefaultAsync();
            if (model.WeekScheduleName == null)
                model.WeekScheduleName = "Без названия";
            var NewWeekSchedule = new EWeekSchedule()
            {
                Name = model.WeekScheduleName,
                Speciality = spec,
                Schedule = new List<EDaySchedule>(6)
            };
            // DBContext.WeekSchedules.Add(NewWeekSchedule);

            NewWeekSchedule.Schedule.Add(new EDaySchedule()//mon
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 0,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for(int i=0;i<8;i++)
            {
                var discipline = model.mon[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[0];
                if(discipline.DisciplineId.ToString()=="-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";

                NewWeekSchedule.Schedule[0].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add( new EDaySchedule()//tue
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 1,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.tue[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[1];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[1].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add(  new EDaySchedule()//wed
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 2,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.wed[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[2];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[2].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add( new EDaySchedule()//thu
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 3,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.thu[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[3];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[3].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add( new EDaySchedule()//fri
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 4,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.fri[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[4];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[4].DisciplinesForDay.Add(discipline);
            }

            NewWeekSchedule.Schedule.Add( new EDaySchedule()//sat
            {
                WeekSchedule = NewWeekSchedule,
                DayNumber = 5,
                DisciplinesForDay = new List<EScheduleItem>(8)
            });
            //DBContext.DaySchedules.Add(NewWeekSchedule.Schedule[0]);
            for (int i = 0; i < 8; i++)
            {
                var discipline = model.sat[i];
                discipline.DaySchedule = NewWeekSchedule.Schedule[5];
                if (discipline.DisciplineId.ToString() == "-1")
                {
                    discipline.Name = "Нет пары";
                    discipline.TeacherFIO = "";
                    discipline.Type = "";
                }
                else
                {
                    var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher)
                                      where di.Id == discipline.DisciplineId
                                      select di).FirstOrDefaultAsync();
                    discipline.Name = disc.Name;
                    discipline.TeacherFIO = disc.Teacher.FIO;
                }
                if (discipline.Classroom == null)
                    discipline.Classroom = "";
                NewWeekSchedule.Schedule[5].DisciplinesForDay.Add(discipline);
            }
            spec.Schedules.Add(NewWeekSchedule);
            DBContext.Add(NewWeekSchedule);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("Index", new { SpecId = spec.Id });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteSchedule(string WeekScheduleId)
        {
            var sched = await (from s in DBContext.WeekSchedules.Include(s=>s.Speciality)
                               where s.id.ToString() == WeekScheduleId
                               select s).FirstOrDefaultAsync();
            var spec = await (from s in DBContext.Specialities.Include(s => s.Schedules)
                              where s.Id == sched.Speciality.Id
                              select s).FirstOrDefaultAsync();
            
            DBContext.Remove(sched);
            spec.Schedules.Remove(sched);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("Index", new { SpecId = spec.Id });
        }
        public async Task<IActionResult> Create(string SpecId)
        {
            var spec = await (from sp in DBContext.Specialities.Include(d => d.Disciplines)
                              where sp.Id.ToString() == SpecId
                              select sp).FirstOrDefaultAsync();
           
                var model = new CreateDisciplineViewModel()
                {
                    SpecialityId = spec.Id.ToString(),
                    AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher")
                };
                return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateDisciplineViewModel model)
        {
            if(ModelState.IsValid)
            { 
            var teacher = await userManager.FindByIdAsync(model.TeacherId);
            var spec = await (from s in DBContext.Specialities.Include(d => d.Disciplines)
                              where s.Id.ToString() == model.SpecialityId
                              select s).FirstOrDefaultAsync();
            var name = model.Name;
            var info = "";
            if (!String.IsNullOrWhiteSpace(model.Info))
                info = model.Info;
            
            
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
            model.AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher");
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
            if (ModelState.IsValid) { 
            var teacher = await userManager.FindByIdAsync(model.TeacherId);
            string name = model.Name;
            var info = "";
            if (!String.IsNullOrWhiteSpace(model.Info))
                info = model.Info;
            
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
            model.AvailableTeachers = await userManager.GetUsersInRoleAsync("Teacher");
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
                DisciplineFilesChecker checker = new DisciplineFilesChecker();
                checker.Check(environment, DBContext);

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
        public IActionResult AddStudentUNF(string SpecId, string ErrorText)
        {
            if (ErrorText != null)
                TempData["ErrorText"] = ErrorText;
            return RedirectToAction("AddStudent", new { Filled = false, TSpecId=SpecId});
        }
        public async Task<IActionResult> AddStudent(bool Filled, string TSpecId, string CSpecId, string CSpecName,
            string uEmail, string uFIO)
        {
            if(!Filled)
            {
                ViewBag.Mode = "Unfilled";
                AddStudentViewModel model1 = new AddStudentViewModel()
                {
                    AvailableStudents = await userManager.GetUsersInRoleAsync("Student"),
                    context = DBContext,
                    TargetSpecId = TSpecId
                };
                return View(model1);
            }
            
                ViewBag.Mode = "Filled";
                AddStudentViewModel model2 = new AddStudentViewModel()
                {
                    AvailableStudents = await userManager.GetUsersInRoleAsync("Student"),
                    context = DBContext,
                    TargetSpecId = TSpecId,
                    CurrentSpecId=CSpecId,
                    CurrentSpec=CSpecName,
                    FIO=uFIO,
                    Email=uEmail
                };
                return View(model2);
            
            
        }

        [HttpPost]
        public async Task<IActionResult> FillData(AddStudentViewModel model)
        {
            if (ModelState.IsValid) { 
            model.context = DBContext;
            await model.FillDataAsync();
            var user = await (from u in DBContext.Users where u.NormalizedEmail == model.Email.ToUpper() select u).FirstOrDefaultAsync();
            if (user == null || !(await userManager.IsInRoleAsync(user, "Student")))
            {
                string err = "Пользователь не является студентом";//костыль ебаный
                return RedirectToAction("AddStudentUNF", new { SpecId = model.TargetSpecId, ErrorText = err });
            }
            return RedirectToAction("AddStudent", new
            {
                Filled = true,
                TSpecId = model.TargetSpecId,
                CSpecId = model.CurrentSpecId,
                CSpecName = model.CurrentSpec,
                uEmail = model.Email,
                uFIO = model.FIO
            });
        }
            else
            {
                string err = "Не указан Email";//костыль ебаный
                return RedirectToAction("AddStudentUNF", new { SpecId = model.TargetSpecId, ErrorText = err });
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddStudent(AddStudentViewModel model)
        {
            var md = model;
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

                    using (var fileStream = new FileStream(environment.WebRootPath + path, FileMode.Create))
                    {
                        await uploadedFile.CopyToAsync(fileStream);
                    }
                    // RegFile filereg = new RegFile { Name = uploadedFile.FileName, Path = enviroment.WebRootPath + path };
                    return RedirectToAction("MassRegistration", new { Path = (environment.WebRootPath + path), Id=SpecId });
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