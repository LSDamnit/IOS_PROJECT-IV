using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class CreateInstitutionViewModel
    {
       public string Name { get; set; }
        
        public IList<EUser> AvailableManagers { get; set; }        
        public string ManagerId { get; set; }
    }
}
