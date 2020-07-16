using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class SpecialitiesViewModel
    {
        public string DepartmentId { get; set; }
        public string InstId { get; set; }
        public string InstManagerId { get; set; }
        public string InstManagerEmail { get; set; }
        public string HeadTeacherId { get; set; }
        public string HeadTeacherEmail { get; set; }
        public string DepartmentName { get; set; }
        public IList<ESpeciality> Specialities { get; set; }
        public List<string> userGrants { get; set; }
    }
}
