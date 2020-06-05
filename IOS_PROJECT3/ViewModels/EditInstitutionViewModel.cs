using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class EditInstitutionViewModel
    {
        public string Name { get; set; }
        public string Idinst { get; set; }
        public IList<EUser> AvailableManagers { get; set; }
        public string ManagerId { get; set; }
       // public EInstitution Institution { get; set; }
    }
}
