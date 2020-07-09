using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EWeekSchedule
    {
        public int id { get; set; }
        public string Name { get; set; }
        public ESpeciality Speciality { get; set; }
        public List<EDaySchedule> Schedule { get; set; }
    }
}

