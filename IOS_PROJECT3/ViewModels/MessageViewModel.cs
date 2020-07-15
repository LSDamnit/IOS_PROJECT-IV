using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class MessageViewModel
    {

        public string Subject { get; set; }
        public string Message { get; set; }
        public string ContainerId { get; set; }
        public string ContainerType { get; set; }
        public string SenderEmail { get; set; }
       // public string SenderRole { get; set; }
        public bool SendToStaff { get; set; }
        public bool SendToStudents { get; set; }
        public IList<EUser> Receivers { get; set; }
    }
}
