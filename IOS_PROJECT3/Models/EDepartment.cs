using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EDepartment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual EUser HeadTeacher { get; set; }
        public virtual List<ESpeciality> Specialities { get; set; }
    }
}
