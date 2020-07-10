using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EForumEndopoint
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public EForumNode ParentNode { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public List<EForumFile> PinnedFiles { get; set; }
    }
}
