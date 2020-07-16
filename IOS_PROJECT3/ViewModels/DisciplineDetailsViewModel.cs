using IOS_PROJECT3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class DisciplineDetailsViewModel
    {
        public string DisciplineId { get; set; }
        public string DisciplineName { get; set; }
        public string DisciplineInfo { get; set; }
        public string TeacherId { get; set; }
        public string TeacherEmail { get; set; }
        public string TeacherName { get; set; }
        public string LecH { get; set; }
        public string PracH { get; set; }
        public string ExamType { get; set; }
        public string InstId { get; set; }
        public string InstManagerId { get; set; }
        public string InstManagerEmail { get; set; }
        public string SpecialityId { get; set; }
        public IList<EFile> LectionFiles { get; set; }
        public IList<EFile> PracticeFiles { get; set; }
        public List<string> userGrants { get; set; }
        public bool FromPP { get; set; }

    }
}
