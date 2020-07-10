using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EForumNode
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public string Name { get; set; }
        public EForumNode ParentNode { get; set; }
        public List<EForumNode> ChildNodes { get; set; }
        public List<EForumEndopoint> ChildEndpoints { get; set; }
    }
}
