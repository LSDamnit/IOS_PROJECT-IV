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
                if ((model.EndpointText.IndexOf("<script>") != -1)|| (model.EndpointText.IndexOf("</script>") != -1))
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

                string outfolder = environment.WebRootPath + "/ForumFiles/EPFiles/" + model.EndpointName + "_"
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
                //return RedirectToAction("ForumNode", new { NodeId = model.ParentNodeId });
                return RedirectToAction("ForumEndpoint", new { EndpointId = NewEndpoint.Id });
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

                if((model.CommentText.IndexOf("<script>") != -1)|| (model.CommentText.IndexOf("</script>") != -1))
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

                string outfolder = environment.WebRootPath + "/ForumFiles/CFiles/" + model.EndpointName + "_"
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
        public async Task<IActionResult> EditForumEndpoint(string EndpointId)
        {
            
            var endpoint = await (from e in DBContext.ForumEndpoints.Include(f => f.PinnedFiles)
                                      where e.Id.ToString() == EndpointId
                                      select e).FirstOrDefaultAsync();
            var model = new EditForumEndpointViewModel()
            {
                EndpointName = endpoint.Name,
                EndpointId = endpoint.Id.ToString(),
                EndpointText = endpoint.Text.Replace("<br>", "\n"),
                PinnedFiles = endpoint.PinnedFiles,
                CreatorEmail = endpoint.CreatorEmail
            };
                return View(model);
            
        }
        [HttpPost]
        public async Task<IActionResult> EditForumEndpoint(EditForumEndpointViewModel model)
        {
            if(ModelState.IsValid)
            {
                var endpoint = await (from e in DBContext.ForumEndpoints.Include(f => f.PinnedFiles)
                                      where e.Id.ToString() == model.EndpointId
                                      select e).FirstOrDefaultAsync();
                if ((model.EndpointText.IndexOf("<script>") != -1)|| (model.EndpointText.IndexOf("</script>") != -1))
                {
                    ModelState.AddModelError("scripts", "Теги <script> запрещены!");
                    return View(model);
                }
                DBContext.ForumEndpoints.Update(endpoint).Entity.Name = model.EndpointName;
                var safeText = model.EndpointText.Replace("<script>", "");//на всякий
                safeText = safeText.Replace("\n", "<br>");
                DBContext.ForumEndpoints.Update(endpoint).Entity.Text = safeText;
                if(model.UploadedFiles!=null)
                {
                    string outfolder;
                    if ((endpoint.PinnedFiles != null) && (endpoint.PinnedFiles.Count > 0))
                    {
                         outfolder = endpoint.PinnedFiles[0].Path.Replace(endpoint.PinnedFiles[0].Name, String.Empty);
                    }
                    else
                    {
                         outfolder = environment.WebRootPath + "/ForumFiles/EPFiles/" + model.EndpointName + "_"
                        + System.DateTime.Now.ToString("s").Replace(":", "-") + "/";
                        if (!Directory.Exists(outfolder))
                        {
                            Directory.CreateDirectory(outfolder);
                        }
                    }
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
                            ForumEndpoint = endpoint
                        };
                        DBContext.ForumFiles.Add(efile);
                        DBContext.ForumEndpoints.Update(endpoint).Entity.PinnedFiles.Add(efile);
                    }
                }
                await DBContext.SaveChangesAsync();
                return RedirectToAction("ForumEndpoint", new { EndpointId = model.EndpointId });
            }
            return View(model);
        }
        public async Task<IActionResult> EditForumComment(string CommentId)
        {
            var comment = await (from e in DBContext.ForumComments
                                 .Include(f => f.PinnedFiles)
                                 .Include(e=>e.ParentEndpoint)
                                  where e.Id.ToString() == CommentId
                                  select e).FirstOrDefaultAsync();
            var model = new EditForumCommentViewModel()
            {
              CommentId=comment.Id.ToString(),
              EndpointId=comment.ParentEndpoint.Id.ToString(),
              CommentText=comment.Text.Replace("<br>","\n"),
              PinnedFiles=comment.PinnedFiles,
              CommentCreatorEmail=comment.CreatorEmail
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditForumComment(EditForumCommentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var comment = await (from e in DBContext.ForumComments.Include(f => f.PinnedFiles)
                                      where e.Id.ToString() == model.CommentId
                                      select e).FirstOrDefaultAsync();
                var parentEndpoint = await (from e in DBContext.ForumEndpoints.Include(c => c.Comments)
                                      where e.Id.ToString() == model.EndpointId
                                      select e).FirstOrDefaultAsync();
                
                if ((model.CommentText.IndexOf("<script>") != -1)||(model.CommentText.IndexOf("</script>") != -1))
                {
                    ModelState.AddModelError("scripts", "Теги <script> запрещены!");
                    return View(model);
                }
                var safeText = model.CommentText.Replace("<script>", "");//на всякий
                safeText = safeText.Replace("\n", "<br>");
                safeText += "<br><i id='upd'>Отредактирован " + System.DateTime.Now.ToString("d") + "</i>";
                DBContext.ForumComments.Update(comment).Entity.Text = safeText;
                if (model.CommentUploadedFiles != null)
                {
                    string outfolder;
                    if ((comment.PinnedFiles != null) && (comment.PinnedFiles.Count > 0))
                    {
                        outfolder = comment.PinnedFiles[0].Path.Replace(comment.PinnedFiles[0].Name, String.Empty);
                    }
                    else
                    {
                        outfolder = environment.WebRootPath + "/ForumFiles/CFiles/" + parentEndpoint.Name + "_"
                       + System.DateTime.Now.ToString("s").Replace(":", "-") + "/";
                        if (!Directory.Exists(outfolder))
                        {
                            Directory.CreateDirectory(outfolder);
                        }
                    }
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
                            ForumComment = comment
                        };
                        DBContext.ForumFiles.Add(efile);
                        DBContext.ForumComments.Update(comment).Entity.PinnedFiles.Add(efile);
                    }
                }
                await DBContext.SaveChangesAsync();
                return RedirectToAction("ForumEndpoint", new { EndpointId = model.EndpointId });
            }
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteForumComment(string CommentId)
        {
            var comment = await (from c in DBContext.ForumComments
                                 .Include(f => f.PinnedFiles)
                                 .Include(e=>e.ParentEndpoint)
                                 where c.Id.ToString() == CommentId
                                 select c).FirstOrDefaultAsync();
            if(comment.PinnedFiles!=null)
            {
                foreach(var f in comment.PinnedFiles)
                {
                    DBContext.ForumFiles.Remove(f);
                    System.IO.File.Delete(f.Path);
                    try
                    {
                        Directory.Delete(f.Path.Replace(f.Name, String.Empty));
                    }
                    catch (Exception e) { }
                }
            }
            DBContext.ForumComments.Remove(comment);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("ForumEndpoint", new { EndpointId = comment.ParentEndpoint.Id });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteForumNode(string NodeId)
        {
            var node = await (from n in DBContext.ForumNodes
                              .Include(n => n.ChildNodes)
                              .Include(c => c.ChildEndpoints)
                              .Include(p=>p.ParentNode)
                              where n.Id.ToString() == NodeId
                              select n).FirstOrDefaultAsync();
            var cid = node.CreatorId;
            var pid = node.ParentNode.Id.ToString();
            await DeleteAllSubNodes(NodeId);
            await DBContext.SaveChangesAsync();
            if (node.CreatorId == "-1")
                return RedirectToAction("ForumNode", new { NodeId = "1" });
            return RedirectToAction("ForumNode", new { NodeId = pid });

        }
        public async Task DeleteAllSubNodes(string NodeId)
        {
            var node = await (from n in DBContext.ForumNodes
                              .Include(n => n.ChildNodes)
                              .Include(c => c.ChildEndpoints)
                              where n.Id.ToString() == NodeId
                              select n).FirstOrDefaultAsync();
            if ((node.ChildEndpoints != null) && (node.ChildEndpoints.Count > 0))
            {
                foreach (var e in node.ChildEndpoints)
                {
                    await DeleteAllFilesOnEndpointAsync(e.Id.ToString());
                }
            }
            if ((node.ChildNodes != null) && (node.ChildNodes.Count > 0))
            {
                foreach(var subnode in node.ChildNodes)
                {
                    var full = await (from n in DBContext.ForumNodes
                              .Include(n => n.ChildNodes)
                              .Include(c => c.ChildEndpoints)
                                      where n.Id == subnode.Id
                                      select n).FirstOrDefaultAsync();
                    if ((subnode.ChildEndpoints != null) && (subnode.ChildEndpoints.Count > 0))
                    {
                        foreach (var e in node.ChildEndpoints)
                        {
                            await DeleteAllFilesOnEndpointAsync(e.Id.ToString());
                        }
                    }
                    if ((subnode.ChildNodes != null) && (subnode.ChildNodes.Count > 0))
                    {
                        foreach(var subsubnode in subnode.ChildNodes)
                        {
                          await  DeleteAllSubNodes(subsubnode.Id.ToString());
                        }
                    }
                    DBContext.ForumNodes.Remove(subnode);
                }
            }
            DBContext.ForumNodes.Remove(node);
            //await DBContext.SaveChangesAsync();
        }
        [HttpPost]
        public async Task<IActionResult> DeleteForumEndpoint(string EndpointId)
        {
            await DeleteAllFilesOnEndpointAsync(EndpointId);
            var endpoint = await (from e in DBContext.ForumEndpoints.Include(p=>p.ParentNode)
                                  where e.Id.ToString() == EndpointId
                                  select e).FirstOrDefaultAsync();
            DBContext.ForumEndpoints.Remove(endpoint);
            await DBContext.SaveChangesAsync();
            return RedirectToAction("ForumNode", new { NodeId = endpoint.ParentNode.Id.ToString() });
        }
        public async Task DeleteAllFilesOnEndpointAsync(string EndpointId)
        {
            var endpoint = await (from e in DBContext.ForumEndpoints
                                  .Include(f => f.Comments)
                                  .Include(f => f.PinnedFiles)
                                  where e.Id.ToString() == EndpointId
                                  select e).FirstOrDefaultAsync();
            var comments = await (from c in DBContext.ForumComments
                                  .Include(f => f.PinnedFiles)
                                  .Include(p => p.ParentEndpoint)
                                  where c.ParentEndpoint.Id.ToString() == EndpointId
                                  select c).ToListAsync();
            if((endpoint.PinnedFiles!=null)&&(endpoint.PinnedFiles.Count>0))
            {
                string folder="";
                foreach (var file in endpoint.PinnedFiles)
                {
                    DBContext.ForumFiles.Remove(file);
                   // endpoint.PinnedFiles.Remove(file);
                    System.IO.File.Delete(file.Path);
                    folder = file.Path.Replace(file.Name, String.Empty);
                }
                Directory.Delete(folder, true);
            }
            foreach(var comment in comments)
            {
                if ((comment.PinnedFiles != null) && (comment.PinnedFiles.Count > 0))
                {
                    string folder = "";
                    foreach (var file in comment.PinnedFiles)
                    {
                        DBContext.ForumFiles.Remove(file);
                        //comment.PinnedFiles.Remove(file);
                        System.IO.File.Delete(file.Path);
                        folder = file.Path.Replace(file.Name, String.Empty);
                    }
                    Directory.Delete(folder, true);
                }
            }
            await DBContext.SaveChangesAsync();
            
        }
        public async Task<IActionResult> DeletePinnedFile(string FileId, string ParentType)//1=EP 2=C
        {
            //добавить ветвление после добавления метода редактирования коммента
            if(ParentType=="1")
            {
                var file = await (from f in DBContext.ForumFiles.Include(f => f.ForumEndpoint)
                              .ThenInclude(e => e.PinnedFiles)
                                  where f.Id.ToString() == FileId
                                  select f).FirstOrDefaultAsync();
                var endpoint = file.ForumEndpoint;
                endpoint.PinnedFiles.Remove(file);
                DBContext.ForumFiles.Remove(file);
                System.IO.File.Delete(file.Path);
                try
                {
                    Directory.Delete(file.Path.Replace(file.Name, String.Empty));
                }
                catch (Exception e) { }

                await DBContext.SaveChangesAsync();
                return RedirectToAction("EditForumEndpoint", new { EndpointId = endpoint.Id });
            }
            else if(ParentType=="2")
            {
                var file = await (from f in DBContext.ForumFiles.Include(f => f.ForumComment)
                                              .ThenInclude(e => e.PinnedFiles)
                                  where f.Id.ToString() == FileId
                                  select f).FirstOrDefaultAsync();
                var comment = file.ForumComment;
                comment.PinnedFiles.Remove(file);
                DBContext.ForumFiles.Remove(file);
                System.IO.File.Delete(file.Path);
                try
                {
                    Directory.Delete(file.Path.Replace(file.Name, String.Empty));
                }
                catch (Exception e) { }

                await DBContext.SaveChangesAsync();
                return RedirectToAction("EditForumComment", new { CommentId = comment.Id });
            }
            throw new Exception("Invalid ParentType argument");
        }
        public async Task<IActionResult> EditForumNode(string NodeId)
        {
            var node = await (from n in DBContext.ForumNodes
                              where n.Id.ToString() == NodeId
                              select n).FirstOrDefaultAsync();
            var model = new EditForumNodeViewModel()
            {
                NodeId = node.Id.ToString(),
                NodeName = node.Name
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditForumNode(EditForumNodeViewModel model)
        {
            if(ModelState.IsValid)
            {
                var node = await (from n in DBContext.ForumNodes
                                  where n.Id.ToString() == model.NodeId
                                  select n).FirstOrDefaultAsync();
                DBContext.ForumNodes.Update(node).Entity.Name = model.NodeName;
                await DBContext.SaveChangesAsync();
                return RedirectToAction("ForumNode", new { NodeId = model.NodeId });
            }
            return View(model);

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