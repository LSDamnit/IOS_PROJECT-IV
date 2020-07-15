using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class CreateComplainViewModel
    {
        public int PageLoads { get; set; }
        [Required(ErrorMessage = "Не указана тема жалобы")]
        public string Subject { get; set; }
        [Required(ErrorMessage = "Ничего не написано в тексте жалобы")]
        public string Text { get; set; }
        public IFormFileCollection UploadedFiles { get; set; }
        public string CreatorEmail { get; set; }
        public bool Anon { get; set; }
    }
}
