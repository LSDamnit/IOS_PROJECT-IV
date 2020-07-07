using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class CreateInstitutionViewModel
    {
        [Required(ErrorMessage = "Не указано название")]
        public string Name { get; set; }
        
        public IList<EUser> AvailableManagers { get; set; }
        //[Required]
        //[Display(Name = "Менеджер")]
        public string ManagerId { get; set; }
    }
}
