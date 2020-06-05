using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class EditDepartmentViewModel
    {
        public string Name { get; set; }
        public string DepartmentId { get; set; }
        public string HeadTeacherId { get; set; }
        public string InstitutionId { get; set; }
       // public string ManagerId { get; set; }
       // public string ManagerEmail { get; set; }
        public string InstitutionName { get; set; }
        public IList<EUser> AvailableTeachers { get; set; }
    }
}
