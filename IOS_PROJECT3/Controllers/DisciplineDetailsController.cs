using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.Grants;
using IOS_PROJECT3.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace IOS_PROJECT3.Controllers
{
    public class DisciplineDetailsController : Controller
    {
        DBMergedContext DBContext;
        IWebHostEnvironment Environment;
        GrantCheckService checkService;
        public DisciplineDetailsController(GrantCheckService checkService, DBMergedContext context, IWebHostEnvironment environment)
        {
            this.checkService = checkService;
            DBContext = context;
            Environment = environment;
        }
        public async Task<IActionResult> Index(string DiscId,bool FileError)
        {
            var disc = await (from di in DBContext.Disciplines.Include(t => t.Teacher).Include(f => f.Files) where di.Id.ToString() == DiscId select di).FirstOrDefaultAsync();
            var spec = await (from s in DBContext.Specialities.Include(s => s.Disciplines)
                              where s.Disciplines.Contains(disc)
                              select s).FirstOrDefaultAsync();
            var dep = await (from d in DBContext.Departments.Include(s => s.Specialities) where d.Specialities.Contains(spec)
                             select d).FirstOrDefaultAsync();
            var inst = await (from i in DBContext.Institutions.Include(s => s.Departments).Include(m => m.Manager)
                              where i.Departments.Contains(dep)
                              select i).FirstOrDefaultAsync();
            var difiles = await (from f in DBContext.Files.Include(u => u.UserLoad) where disc.Files.Contains(f) select f).ToListAsync();
            var model = new DisciplineDetailsViewModel()
            {
                DisciplineId = disc.Id.ToString(),
                DisciplineName = disc.Name,
                TeacherId = disc.Teacher.Id,
                TeacherEmail = disc.Teacher.Email,
                InstId = inst.Id.ToString(),
                InstManagerId = inst.Manager.Id.ToString(),
                InstManagerEmail = inst.Manager.Email,
                LectionFiles = new List<EFile>(),
                PracticeFiles = new List<EFile>(),
                //DisciplineInfo=String.Join("<p></p>",disc.About.Split("\n")),
                DisciplineInfo = disc.About,
                LecH = disc.LectionH.ToString(),
                PracH = disc.PracticeH.ToString(),
                ExamType = disc.ExamType,
                TeacherName = disc.Teacher.FIO,
                userGrants = await checkService.getUserGrants(User),
                SpecialityId = spec.Id.ToString(),
                SpecialityId = spec.Id.ToString()
            };
            foreach (var f in difiles)
            {
                if (f.Tag == "lection")
                    model.LectionFiles.Add(f);
                else if (f.Tag == "practice")
                    model.PracticeFiles.Add(f);
            }
            if(FileError)
            {
                ViewBag.FileError = true;
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile upload, string tag, string id, string user)
        {
            //здесь пока что нет вообще никакой безопасности
            if (upload != null)
            {
                if(upload.Length>= 10485760)
                {
                    return RedirectToAction("Index", new { DiscId = id, FileError=true });
                }
                var euser = await (from u in DBContext.Users where u.Email.ToLower() == user.ToLower() select u).FirstOrDefaultAsync();
                string t = tag;
                string outpath = Environment.WebRootPath + "/DisciplineFiles/" + "id" + id + "/" + tag + "/";
                if (!Directory.Exists(outpath))
                    Directory.CreateDirectory(outpath);
                outpath += upload.FileName;
                using (var fileStream = new FileStream(outpath, FileMode.Create))
                {
                    await upload.CopyToAsync(fileStream);
                }
                var efile = new EFile()
                {
                    DateLoad = DateTime.Today.ToShortDateString(),
                    UserLoad = euser,
                    Tag = tag,
                    Path = outpath,
                    Name = upload.FileName
                };
                var disc = await (from di in DBContext.Disciplines.Include(f => f.Files)
                                  where di.Id.ToString() == id
                                  select di).FirstOrDefaultAsync();
                DBContext.Update(disc).Entity.Files.Add(efile);
                await DBContext.SaveChangesAsync();
            }
            return RedirectToAction("Index", new { DiscId = id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFile(string FileId)
        {
            var file = await (from f in DBContext.Files where f.Id.ToString() == FileId select f).FirstOrDefaultAsync();
            var disc = await (from di in DBContext.Disciplines.Include(f => f.Files)
                              where di.Files.Contains(file) select di).FirstOrDefaultAsync();
            if (Directory.Exists(file.Path.Replace("/", @"\"))) ;
            {
                System.IO.File.Delete(file.Path);
            }
            DBContext.Update(disc).Entity.Files.Remove(file);
            DBContext.Remove(file);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("Index", new { DiscId = disc.Id});
        }

        public async Task<IActionResult> DownloadFile(string FileId)
        {
            var file = await (from f in DBContext.Files where f.Id.ToString() == FileId select f).FirstOrDefaultAsync();
            string ftype = "application/octet-stream";
            string fname = file.Name;
            string fpath = file.Path;
            return PhysicalFile(fpath, ftype, fname);

        }
    }
}