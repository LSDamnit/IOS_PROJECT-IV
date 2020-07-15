using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
namespace IOS_PROJECT3.ViewModels
{
    public class ComplainDetailsViewModel
    {
        public List<EComplainFile> Files { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string ComplainId { get; set; }//--
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string CreatorEmail { get; set; }
        public string CreationDateString { get; set; }
        public int Checked { get; set; }
        public string CheckedBy_Fio { get; set; }
        public string CheckedBy_Email { get; set; }
    }
}
