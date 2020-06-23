using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }      
        public string Tag { get; set; }//lections/practice
        public virtual EUser UserLoad { get; set; }
        public  string DateLoad { get; set; }
        public EDiscipline Discipline { get; set; }
    }
}
