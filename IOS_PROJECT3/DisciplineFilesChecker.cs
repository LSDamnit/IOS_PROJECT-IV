using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace IOS_PROJECT3
{
    public class DisciplineFilesChecker
    {

        public void Check(IWebHostEnvironment env, DBMergedContext dbc)
        {
            string checkpath = env.WebRootPath + "/DisciplineFiles/";
            string[] subdirs = Directory.GetDirectories(checkpath);

            foreach(var dir in subdirs)
            {
                if(dir.Replace(checkpath, "").StartsWith("id"))
                {  
                 var id = dir.Replace(checkpath,"").Remove(0, 2);
                    EDiscipline disc = (from di in dbc.Disciplines
                                              where di.Id.ToString() == id
                                              select di).FirstOrDefault();
                   if(disc==null)
                    {
                        Directory.Delete(dir, true);
                    }
                }
            }
        }
    }
}
