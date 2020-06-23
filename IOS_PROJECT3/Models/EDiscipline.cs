using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EDiscipline
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual EUser Teacher { get; set; }
        public string ExamType { get; set; }
        public int LectionH { get; set; }
        public int PracticeH { get; set; }
        public string About { get; set; }
        public virtual List<EFile> Files { get; set; }
        public ESpeciality Speciality { get; set; }
    }
}
