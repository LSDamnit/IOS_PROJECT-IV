using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EDaySchedule
    {
        public int id { get; set; }
        public EWeekSchedule WeekSchedule { get; set; }
        public int DayNumber { get; set; }//mon=0; tuesd=1;...
        public List<EScheduleItem> DisciplinesForDay { get; set; }
    }
}
