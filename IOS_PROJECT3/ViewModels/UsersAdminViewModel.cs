using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using IOS_PROJECT3.Grants;
using Microsoft.EntityFrameworkCore;

namespace IOS_PROJECT3.ViewModels
{
    public class UsersAdminViewModel
    {
        DBMergedContext DBC;
        public UsersAdminViewModel(DBMergedContext db)
        {
            DBC = db;
        }
        public IEnumerable<EUser> Users {get;set;}
        public IDictionary<EUser, string> WhyTitles { get; set; }
        public List<string> userGrants { get; set; }

        public bool IsDeletable(EUser u)
        {
            var ins = (from i in DBC.Institutions.Include(m => m.Manager) where i.Manager.Id == u.Id select i.Id).Any();
            var dep = (from d in DBC.Departments.Include(h => h.HeadTeacher) where d.HeadTeacher.Id == u.Id select d.Id).Any();
            var dis = (from di in DBC.Disciplines.Include(t => t.Teacher) where di.Teacher.Id == u.Id select di.Id).Any();

            
            if (!ins && !dep && !dis)
                return true;
            else
            {
                WhyTitles[u] = "Удаление невозможно, список причин:\n";
                if (ins)
                    WhyTitles[u] += "Пользователь является менеджером института\n";
                if (dep)
                    WhyTitles[u] += "Пользователь является заведующим кафедры\n";
                if (dis)
                    WhyTitles[u] += "Пользователь является преподавателем дисциплины\n";
                WhyTitles[u] += "Закройте данные связи, после чего удаление станет доступно";

            }
            return false;
        }
    }
}
