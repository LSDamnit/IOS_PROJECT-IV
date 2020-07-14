using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.Controllers
{
    public class ComplainsController : Controller
    {
        private DBMergedContext DBContext;
        // UserManager<EUser> userManager;
        IWebHostEnvironment environment;
        public ComplainsController(DBMergedContext context, /*UserManager<EUser> userManager,*/ IWebHostEnvironment environment)
        {
            DBContext = context;
            //this.userManager = userManager;
            this.environment = environment;
        }
        public IActionResult Index()
        {
            var model = (from c in DBContext.Complains select c).OrderBy(c=>c.Checked).ThenByDescending(p => p.CreationDate).ToList();
            return View(model);
        }

        public IActionResult CreateComplain(string CreatorEmail)
        {
            var ce = CreatorEmail;
            var model = new CreateComplainViewModel()
            {
                CreatorEmail = ce
            };
            return View(model);
        }
        public async Task<IActionResult> ComplainDetails(string ComplainId)
        {
            var complain = await (from f in DBContext.Complains
                                 .Include(f => f.PinnedFiles)
                                  where f.Id.ToString() == ComplainId
                                  select f).FirstOrDefaultAsync();
            if(complain!=null)
            {
                var model = new ComplainDetailsViewModel()
                {
                    Subject = complain.Subject,
                    Text = complain.Text,
                    ComplainId = complain.Id.ToString(),
                    CreatorId = complain.CreatorId,
                    CreatorEmail = complain.CreatorEmail,
                    CreatorName = complain.CreatorFio,
                    CreationDateString = complain.CreationDate.ToString("dd'.'MM'.'yyyy HH:mm:ss"),
                    Files = complain.PinnedFiles,
                    Checked = complain.Checked,
                    CheckedBy_Email=complain.CheckedBy_Email,
                    CheckedBy_Fio=complain.CheckedBy_Fio
                };
                
                return View(model);
            }
            return RedirectToAction("index");
        }

        [HttpPost]
        public async Task<IActionResult> CreateComplain(CreateComplainViewModel model)
        {
            if (ModelState.IsValid)
            {
               
                    var creator = await (from u in DBContext.Users
                                         where u.NormalizedEmail == model.CreatorEmail.ToUpper()
                                         select u).FirstOrDefaultAsync();

                if ((model.Text.IndexOf("<script>") != -1) || (model.Text.IndexOf("</script>") != -1))
                {
                    ModelState.AddModelError("scripts", "Теги <script> запрещены!");
                    return View(model);
                }
                var safeText = model.Text.Replace("<script>", "");//на всякий
                safeText = safeText.Replace("\n", "<br>");

               
                var NewComplain = new EComplain
                {
                    Subject = model.Subject,
                    Text = safeText,
                    CreationDate = System.DateTime.Now,
                    PinnedFiles = new List<EComplainFile>(),
                    Checked=0
                };
                if(!model.Anon)
                {
                    NewComplain.CreatorId = creator.Id;
                    NewComplain.CreatorEmail = creator.Email;
                    NewComplain.CreatorFio = creator.FIO;
                }
                else
                {
                    NewComplain.CreatorId = "-1";
                }
                 DBContext.Complains.Add(NewComplain);
                 await DBContext.SaveChangesAsync();

                string outfolder = environment.WebRootPath + "/ComplainFiles/" + NewComplain.Id + "/";
                if (!Directory.Exists(outfolder))
                {
                    Directory.CreateDirectory(outfolder);
                }
                if (model.UploadedFiles != null)
                    foreach (IFormFile file in model.UploadedFiles)
                    {
                        var outpath = outfolder + file.FileName;
                        using (var fileStream = new FileStream(outpath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        var efile = new EComplainFile()
                        {
                            Name = file.FileName,
                            Path = outpath,
                            ParentComplain=NewComplain
                        };
                        DBContext.ComplainFiles.Add(efile);
                        DBContext.Complains.Update(NewComplain).Entity.PinnedFiles.Add(efile);
                    }
                
                await DBContext.SaveChangesAsync();
                //return RedirectToAction("ForumNode", new { NodeId = model.ParentNodeId });
                return RedirectToAction("Index","Home");
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteComplain(string ComplainId)
        {
            var complain= await (from f in DBContext.Complains
                                .Include(f => f.PinnedFiles)
                                  where f.Id.ToString() == ComplainId
                                  select f).FirstOrDefaultAsync();
            if((complain.PinnedFiles!=null)&&(complain.PinnedFiles.Count>0))
            {
                foreach (var file in complain.PinnedFiles)
                {
                    DBContext.ComplainFiles.Remove(file);
                   // System.IO.File.Delete(file.Path);
                }
                Directory.Delete(environment.WebRootPath + "/ComplainFiles/" + complain.Id + "/", true);
            }
            DBContext.Complains.Remove(complain);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("index");
        }
        public async Task<IActionResult> MarkComplainChecked(string ComplainId, string UserEmail)
        {
            var complain = await (from f in DBContext.Complains
                                  where f.Id.ToString() == ComplainId
                                  select f).FirstOrDefaultAsync();
            var user = await (from u in DBContext.Users
                              where u.NormalizedEmail == UserEmail.ToUpper()
                              select u).FirstOrDefaultAsync();
            DBContext.Complains.Update(complain).Entity.Checked = 1;
            DBContext.Complains.Update(complain).Entity.CheckedBy_Id = user.Id;
            DBContext.Complains.Update(complain).Entity.CheckedBy_Email = user.Email;
            DBContext.Complains.Update(complain).Entity.CheckedBy_Fio = user.FIO;
            await DBContext.SaveChangesAsync();
            return RedirectToAction("ComplainDetails", new { ComplainId = complain.Id.ToString() });
        }
        public async Task<IActionResult> DownloadFile(string FileId)
        {
            var file = await (from f in DBContext.ComplainFiles where f.Id.ToString() == FileId select f).FirstOrDefaultAsync();
            string ftype = "application/octet-stream";
            string fname = file.Name;
            string fpath = file.Path;
            return PhysicalFile(fpath, ftype, fname);

        }
    }
}