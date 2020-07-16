using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
namespace IOS_PROJECT3.ViewModels
{
    public class CreateScheduleViewModel
    {
        public string SpecialityId { get; set; }
        public string WeekScheduleName { get; set; }

        public IList<EDiscipline> AvailableDisciplines { get; set; }
        public IList<EScheduleItem> mon { get; set; }
        public IList<EScheduleItem> tue { get; set; }
        public IList<EScheduleItem> wed { get; set; }
        public IList<EScheduleItem> thu { get; set; }
        public IList<EScheduleItem> fri { get; set; }
        public IList<EScheduleItem> sat { get; set; }
        public List<string> userGrants { get; set; }

        public void init()
        {
            mon = new List<EScheduleItem>();
            tue = new List<EScheduleItem>();
            wed = new List<EScheduleItem>();
            thu = new List<EScheduleItem>();
            fri = new List<EScheduleItem>();
            sat = new List<EScheduleItem>();
            for(int i=0; i<10;i++)
            {
                mon.Add(new EScheduleItem());
                tue.Add(new EScheduleItem());
                wed.Add(new EScheduleItem());
                thu.Add(new EScheduleItem());
                fri.Add(new EScheduleItem());
                sat.Add(new EScheduleItem());
            }
        }
    }
}
