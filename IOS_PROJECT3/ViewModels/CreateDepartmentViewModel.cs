using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class CreateDepartmentViewModel
    {
        [Required(ErrorMessage ="Не указано название")]
        public string Name { get; set; }
        public string InstId { get; set; }
        public IList<EUser> AvailableTeachers { get; set; }
        public string HeadTeacherId { get; set; }
        public List<string> userGrants { get; set; }
    }
}
