using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EInstitution
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual EUser Manager { get; set; }
        public virtual List<EDepartment> Departments { get; set; }
    }
}
