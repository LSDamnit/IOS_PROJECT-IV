using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class CreateDisciplineViewModel
    {
        public string SpecialityId { get; set; }
        public string Name { get; set; }
        public int LectionH { get; set; }
        public int PracticeH { get; set; }
        public string ExamType { get; set; }
        public IList<EUser> AvailableTeachers { get; set; }
        public string TeacherId { get; set; }
    }
}
