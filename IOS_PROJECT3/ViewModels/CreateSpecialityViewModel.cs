using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace IOS_PROJECT3.ViewModels
{
    public class CreateSpecialityViewModel
    {
        [Required(ErrorMessage = "Не указано название")]
        public string Name { get; set; }

        public string DepId { get; set; }
        public List<string> userGrants { get; set; }
        //public IList<EUser> AvailableTeachers { get; set; }
        // public string HeadTeacherId { get; set; }
    }
}
