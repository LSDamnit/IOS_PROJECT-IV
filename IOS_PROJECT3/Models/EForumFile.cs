using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EForumFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public EForumEndopoint ForumEndpoint { get; set; }
    }
}
