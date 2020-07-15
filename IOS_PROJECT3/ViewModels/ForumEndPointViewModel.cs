using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class ForumEndPointViewModel
    {
        //public List<EForumNode> Nodes { get; set; }
        public List<EForumComment> Comments { get; set; }
        public List<EForumFile> Files { get; set; }
        public string EndpointName { get; set; }
        public string Text { get; set; }
        public string EndpointId { get; set; }//--
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string CreatorEmail { get; set; }
        public string CreationDateString { get; set; }
        public string ParentNodeId { get; set; }
        public List<string> userGrants { get; set; }

        //-------Для написания комммента
        [Required(ErrorMessage = "Ничего не написано в тексте комментария")]
        public string CommentText { get; set; }
        public IFormFileCollection CommentUploadedFiles { get; set; }
        public string CommentCreatorEmail { get; set; }
        //public string CommentCreationDateString { get; set; }
    }
}
