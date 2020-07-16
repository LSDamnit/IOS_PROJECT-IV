using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;

namespace IOS_PROJECT3.ViewModels
{
    public class InstitutionsViewModel
    {
        public IList<EInstitution> Institutions { get; set; }
        public List<string> userGrants { get; set; }

        // public IDictionary<string, string> InstitutionNameManagerFIO { get; set; }
        //public IList<EUser> Managers { get; set; }

    }
}
