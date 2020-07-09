using IOS_PROJECT3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class EditScheduleViewModel
    {
        public int WeekScheduleId { get; set; }
        public string SpecialityId { get; set; }
        public string WeekScheduleName { get; set; }

        public IList<EDiscipline> AvailableDisciplines { get; set; }
        public IList<EScheduleItem> mon { get; set; }
        public IList<EScheduleItem> tue { get; set; }
        public IList<EScheduleItem> wed { get; set; }
        public IList<EScheduleItem> thu { get; set; }
        public IList<EScheduleItem> fri { get; set; }
        public IList<EScheduleItem> sat { get; set; }

        public void init(EWeekSchedule DBSchedule)
        {
            mon = new List<EScheduleItem>(8);
            tue = new List<EScheduleItem>(8);
            wed = new List<EScheduleItem>(8);
            thu = new List<EScheduleItem>(8);
            fri = new List<EScheduleItem>(8);
            sat = new List<EScheduleItem>(8);
            SpecialityId = DBSchedule.Speciality.Id.ToString();
            WeekScheduleName = DBSchedule.Name;
            WeekScheduleId = DBSchedule.id;
            for (int i = 0; i < 8; i++)
            {
                mon.Add(DBSchedule.Schedule[0].DisciplinesForDay[i]);
                tue.Add(DBSchedule.Schedule[1].DisciplinesForDay[i]);
                wed.Add(DBSchedule.Schedule[2].DisciplinesForDay[i]);
                thu.Add(DBSchedule.Schedule[3].DisciplinesForDay[i]);
                fri.Add(DBSchedule.Schedule[4].DisciplinesForDay[i]);
                sat.Add(DBSchedule.Schedule[5].DisciplinesForDay[i]);
            }
        }

    }
}
