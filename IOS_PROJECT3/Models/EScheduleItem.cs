using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EScheduleItem
    {
        public int id { get; set; }
        public int DisciplineId { get; set; }//from disc
        public EDaySchedule DaySchedule { get; set; }
        public string TeacherFIO { get; set; }//from disc
        public string Name { get; set; }//from disc
        public string Classroom { get; set; }
        public string Type { get; set; }//lection or practice
    }
}
