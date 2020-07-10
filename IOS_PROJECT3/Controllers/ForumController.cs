using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.ViewModels;
using Microsoft.AspNetCore.Hosting;
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
                    CreationDateString = forum.CreationDate.ToString("d")
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
    }
}