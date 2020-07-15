using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class CreateForumEndpointViewModel
    {
    [Required(ErrorMessage = "Не указано название")]
    public string EndpointName { get; set; }
    [Required(ErrorMessage = "Ничего не написано в тексте статьи")]
    public string EndpointText { get; set; }
    public IFormFileCollection UploadedFiles { get; set; }
    public string ParentNodeId { get; set; }
    public string CreatorEmail { get; set; }

}
}
