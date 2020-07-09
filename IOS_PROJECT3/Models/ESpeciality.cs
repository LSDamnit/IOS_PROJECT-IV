using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class ESpeciality
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<EDiscipline> Disciplines { get; set; }
        public virtual List<EUser> Students { get; set; }
        public EDepartment Department { get; set; }
        public List<EWeekSchedule> Schedules { get; set; }
        
    }
}
