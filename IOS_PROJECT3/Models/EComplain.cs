using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EComplain
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string CreatorId { get; set; }
        public string CreatorEmail { get; set; }
        public string CreatorFio { get; set; }
        public DateTime CreationDate { get; set; }
        public List<EComplainFile> PinnedFiles { get; set; }
        public int Checked { get; set; } //0=false 1=true
        public string CheckedBy_Id { get; set; }
        public string CheckedBy_Email { get; set; }
        public string CheckedBy_Fio { get; set; }
    }
}
