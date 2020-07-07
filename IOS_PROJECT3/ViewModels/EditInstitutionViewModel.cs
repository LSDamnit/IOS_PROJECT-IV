using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using System.ComponentModel.DataAnnotations;
namespace IOS_PROJECT3.ViewModels
{
    public class EditInstitutionViewModel
    {
        [Required(ErrorMessage = "Не указано название")]
        public string Name { get; set; }
        public string Idinst { get; set; }
        public IList<EUser> AvailableManagers { get; set; } 
        public string ManagerId { get; set; }
    }
}
