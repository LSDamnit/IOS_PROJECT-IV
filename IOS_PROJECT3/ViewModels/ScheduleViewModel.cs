using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class ScheduleViewModel
    {
        public string SpecialityId { get; set; }//для возврата назад
        public string WeekScheduleId { get; set; }
        public string WeekScheduleName { get; set; }
        public Dictionary<string, List<EScheduleItem >> ScheduleForDays { get; set; }

    }
}
