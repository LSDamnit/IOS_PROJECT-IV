using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class DisciplinesViewModel
    {
        public string SpecialityId { get; set; }
        public string DepartmentId { get; set; }
        public string SpecialityName { get; set; }
        public string InstId { get; set; }
        public string InstManagerId { get; set; }
        public string InstManagerEmail { get; set; }
        public IList<EWeekSchedule> Schedules { get; set; }
        public IList<EUser> Students { get; set; }
        public IList<EDiscipline> Disciplines { get; set; }
        public List<string> userGrants { get; set; }
        public bool FromPP { get; set; }
    }
}
