using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.Controllers
{
    public class ForumController : Controller
    {
        private DBMergedContext DBContext;
       // UserManager<EUser> userManager;
        IWebHostEnvironment environment;
        public ForumController(DBMergedContext context, /*UserManager<EUser> userManager,*/ IWebHostEnvironment environment)
        {
            DBContext = context;
            //this.userManager = userManager;
            this.environment = environment;
        }
        public async Task<IActionResult> ForumNode(string NodeId)
        {
            if(NodeId=="1")
            {
                var forum = await (from f in DBContext.ForumNodes
                                   .Include(e => e.ChildEndpoints)
                                   .Include(n => n.ChildNodes)
                                   where f.CreatorId == "-1"
                                   select f).FirstOrDefaultAsync();
                var model = new ForumNodeViewModel()
                {
                    NodeId = forum.Id.ToString(),
                    NodeName=forum.Name,
                    Endpoints = forum.ChildEndpoints.OrderByDescending(d => d.CreationDate).ToList(),
                    Nodes = forum.ChildNodes.OrderByDescending(d => d.CreationDate).ToList(),
                    CreatorId = "-1",
                    CreatorName = "System",
                    CreationDateString = forum.CreationDate.ToString("d"),
                };
                return View(model);
            }
            else
            {
                var forum = await (from f in DBContext.ForumNodes
                                   .Include(e => e.ChildEndpoints)
                                   .Include(n => n.ChildNodes)
                                   where f.Id.ToString()==NodeId
                                   select f).FirstOrDefaultAsync();
                var creator = await (from c in DBContext.Users
                                     where c.Id == forum.CreatorId.ToString()
                                     select c).FirstOrDefaultAsync();
                var model = new ForumNodeViewModel()
                {
                    NodeId = forum.Id.ToString(),
                    NodeName = forum.Name,
                    Endpoints = forum.ChildEndpoints.OrderByDescending(d=>d.CreationDate).ToList(),
                    Nodes = forum.ChildNodes.OrderByDescending(d => d.CreationDate).ToList(),
                    CreatorId = forum.CreatorId.ToString(),
                    CreatorName = creator.FIO,
                    CreatorEmail=creator.Email.ToLower(),
                    CreationDateString = forum.CreationDate.ToString("d")
                };
                return View(model);
            }
            
        }
        public async Task<IActionResult> ForumEndpoint(string EndpointId, List<string> Errors)
        {
            var endpoint = await (from f in DBContext.ForumEndpoints
                                  .Include(e => e.Comments)
                                  .Include(f=>f.PinnedFiles)
                              where f.Id.ToString()==EndpointId
                              select f).FirstOrDefaultAsync();
            foreach(var c in endpoint.Comments)
            {
                var cfiles = await (from f in DBContext.ForumFiles.Include(c => c.ForumComment)
                                    where (f.TypeOfParent == 2 && f.ForumComment.Id == c.Id)
                                    select f).ToListAsync();
                c.PinnedFiles = cfiles;
            }
            var model = new ForumEndPointViewModel()
            {
                Comments=endpoint.Comments,
                Files=endpoint.PinnedFiles,
                EndpointName=endpoint.Name,
                Text=endpoint.Text,
                EndpointId=endpoint.Id.ToString(),
                CreatorId=endpoint.CreatorId,
                CreatorEmail=endpoint.CreatorEmail,
                CreatorName=endpoint.CreatorFio,
                CreationDateString=endpoint.CreationDate.ToString("d")
            };
            if(Errors!=null)
            {
                ViewBag.Errors = Errors;
            }
            return View(model);
        }
        public IActionResult CreateForumEndpoint(string ParentNodeId, string CreatorEmail)
        {
            var p = ParentNodeId;
            var e = CreatorEmail;
            var model = new CreateForumEndpointViewModel()
            {
                ParentNodeId = p,
                CreatorEmail = e,               
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateForumEndpoint(CreateForumEndpointViewModel model)
        {
            if (ModelState.IsValid)
            {
                var creator = await (from u in DBContext.Users
                                     where u.NormalizedEmail == model.CreatorEmail.ToUpper()
                                     select u).FirstOrDefaultAsync();
                var parentNode = await (from n in DBContext.ForumNodes.Include(n => n.ChildEndpoints)
                                        where n.Id.ToString() == model.ParentNodeId
                                        select n).FirstOrDefaultAsync();
                if (model.EndpointText.IndexOf("<script>") != -1)
                {
                    ModelState.AddModelError("scripts", "Теги <script> запрещены!");
                    return View(model);
                }
                var safeText = model.EndpointText.Replace("<script>", "");//на всякий
                safeText = safeText.Replace("\n", "<br>");
                var NewEndpoint = new EForumEndpoint()
                {
                    CreatorId = creator.Id,
                    CreatorEmail = creator.Email,
                    CreatorFio = creator.FIO,
                    ParentNode = parentNode,
                    CreationDate = System.DateTime.Now,
                    Name = model.EndpointName,
                    Text = safeText,//---
                    PinnedFiles = new List<EForumFile>()
                };
               // DBContext.ForumEndpoints.Add(NewEndpoint);
               // await DBContext.SaveChangesAsync();

                string outfolder = environment.WebRootPath + "/EPFiles/" + model.EndpointName + "_"
                    + System.DateTime.Now.ToString("s").Replace(":","-") + "/";
                if(!Directory.Exists(outfolder))
                {
                    Directory.CreateDirectory(outfolder);
                }
                if(model.UploadedFiles!=null)
                foreach (IFormFile file in model.UploadedFiles)
                {
                    var outpath = outfolder + file.FileName;
                    using (var fileStream = new FileStream(outpath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                    var efile = new EForumFile()
                    {
                        Name = file.FileName,
                        Path = outpath,
                        TypeOfParent = 1,
                        ForumEndpoint = NewEndpoint
                    };
                    DBContext.ForumFiles.Add(efile);
                    NewEndpoint.PinnedFiles.Add(efile);
                }
                DBContext.ForumEndpoints.Add(NewEndpoint);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("ForumNode", new { NodeId = model.ParentNodeId });
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateComment(ForumEndPointViewModel model)
        {
            List<string> errors = new List<string>();
            if(ModelState.IsValid)
            {
                var parent = await (from e in DBContext.ForumEndpoints
                                    where e.Id.ToString() == model.EndpointId
                                    select e).FirstOrDefaultAsync();
                var creator = await (from u in DBContext.Users
                                     where u.NormalizedEmail == model.CommentCreatorEmail.ToUpper()
                                     select u).FirstOrDefaultAsync();

                if (model.CommentText.IndexOf("<script>") != -1)
                {
                    errors.Add("Теги <script> запрещены!");
                    return RedirectToAction("ForumEndpoint", new { EndpointId = model.EndpointId, Errors=errors });//<---
                }
                var safeText = model.CommentText.Replace("<script>", "");//на всякий
                safeText = safeText.Replace("\n", "<br>");
                var NewComment = new EForumComment()
                {
                    CreatorId = creator.Id,
                    CreatorEmail = creator.Email,
                    CreatorFio = creator.FIO,
                    CreationDate = System.DateTime.Now,
                    ParentEndpoint = parent,
                    Text = model.CommentText,
                    PinnedFiles = new List<EForumFile>()
                };

                string outfolder = environment.WebRootPath + "/CFiles/" + model.EndpointName + "_"
                    + System.DateTime.Now.ToString("s").Replace(":", "-") + "/";
                if (!Directory.Exists(outfolder))
                {
                    Directory.CreateDirectory(outfolder);
                }
                if (model.CommentUploadedFiles != null)
                    foreach (IFormFile file in model.CommentUploadedFiles)
                    {
                        var outpath = outfolder + file.FileName;
                        using (var fileStream = new FileStream(outpath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        var efile = new EForumFile()
                        {
                            Name = file.FileName,
                            Path = outpath,
                            TypeOfParent = 2,
                            ForumComment = NewComment
                        };
                        DBContext.ForumFiles.Add(efile);
                        NewComment.PinnedFiles.Add(efile);
                    }
                DBContext.ForumComments.Add(NewComment);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("ForumEndpoint", new { EndpointId = model.EndpointId });
            }
            errors.Add("Комментарий не может быть пустым");
            return RedirectToAction("ForumEndpoint", new { EndpointId = model.EndpointId, Errors=errors });
        }
        public IActionResult CreateForumNode(string ParentNodeId, string CreatorEmail)
        {
            var p = ParentNodeId;
            var e = CreatorEmail;
            var model = new CreateForumNodeViewModel()
            {
                ParentNodeId = p,
                CreatorEmail = e
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CreateForumNode(CreateForumNodeViewModel model)
        {
            if(ModelState.IsValid)
            {
                var creator = await (from u in DBContext.Users
                                     where u.NormalizedEmail == model.CreatorEmail.ToUpper()
                                     select u).FirstOrDefaultAsync();
                var parentNode = await (from n in DBContext.ForumNodes.Include(n => n.ChildNodes)
                                        where n.Id.ToString() == model.ParentNodeId
                                        select n).FirstOrDefaultAsync();
                var newNode = new EForumNode()
                {
                    CreatorId = creator.Id,
                    CreatorEmail = creator.Email,
                    CreatorFio = creator.FIO,
                    Name = model.NodeName,
                    CreationDate = DateTime.Now,
                    ParentNode = parentNode
                };
                parentNode.ChildNodes.Add(newNode);
                DBContext.ForumNodes.Add(newNode);
                await DBContext.SaveChangesAsync();
                return RedirectToAction("ForumNode", new { NodeId = model.ParentNodeId });
            }
            return View(model);
        }
        public async Task<IActionResult> DownloadFile(string FileId)
        {
            var file = await (from f in DBContext.ForumFiles where f.Id.ToString() == FileId select f).FirstOrDefaultAsync();
            string ftype = "application/octet-stream";
            string fname = file.Name;
            string fpath = file.Path;
            return PhysicalFile(fpath, ftype, fname);

        }
    }
}