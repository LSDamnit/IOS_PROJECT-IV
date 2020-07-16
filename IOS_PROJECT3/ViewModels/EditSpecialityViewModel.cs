using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class EditSpecialityViewModel
    {
        [Required(ErrorMessage ="Не указано название")]
        public string Name { get; set; }
        public string SpecId { get; set; }
        public string DepId { get; set; }
        public List<string> userGrants { get; set; }
    }
}
