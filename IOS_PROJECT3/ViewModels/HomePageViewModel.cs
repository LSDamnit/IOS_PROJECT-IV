using IOS_PROJECT3.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class HomePageViewModel
    {
        public IList<EInstitution> Institutions { get; }//
        public IList<EDepartment> Departments { get; }//
        public HomePageViewModel(DBMergedContext context)
        {
            Institutions = context.Institutions.ToList();
            Departments = context.Departments.ToList();
        }
    }
}
