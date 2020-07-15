using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class EditForumCommentViewModel
    {
        public string CommentId { get; set; }
        public string EndpointId { get; set; }
        [Required(ErrorMessage = "Ничего не написано в тексте комментария")]
        public string CommentText { get; set; }
        public IFormFileCollection CommentUploadedFiles { get; set; }
        public IList<EForumFile> PinnedFiles { get; set; }
        public string CommentCreatorEmail { get; set; }
    }
}
