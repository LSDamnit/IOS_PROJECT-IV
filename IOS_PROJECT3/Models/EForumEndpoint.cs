using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EForumEndpoint
    {
        public int Id { get; set; }
        public string CreatorId { get; set; }
        public string CreatorEmail { get; set; }
        public string CreatorFio { get; set; }
        public EForumNode ParentNode { get; set; }
        public DateTime CreationDate { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public List<EForumComment> Comments { get; set; }
        public List<EForumFile> PinnedFiles { get; set; }
    }
}
