using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Models
{
    public class EForumComment
    {
        public int Id { get; set; }
        public string CreatorId { get; set; }
        public string CreatorEmail { get; set; }
        public string CreatorFio { get; set; }
        public DateTime CreationDate { get; set; }
        public EForumEndpoint ParentEndpoint { get; set; }
        public string Name { get; set; }//<-----удалить за ненадобностью
        public string Text { get; set; }
        public List<EForumFile> PinnedFiles { get; set; }
    }
}
