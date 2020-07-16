using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
namespace IOS_PROJECT3.ViewModels
{
    public class DepartmentsViewModel
    {
        public string InstitutionId { get; set; }
        public string ManagerId { get; set; }
        public string ManagerEmail { get; set; }
        public string InstitutionName { get; set; }
        public IList<EDepartment> Departments { get; set; }
        public List<string> userGrants { get; set; }
        public bool FromPP { get; set; }

    }
}
