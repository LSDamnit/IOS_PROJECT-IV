using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
namespace IOS_PROJECT3.ViewModels
{
    public class ForumNodeViewModel
    {
        public List<EForumNode> Nodes { get; set; }
        public List<EForumEndpoint> Endpoints { get; set; }
        public string NodeName { get; set; }
        public string NodeId { get; set; }
        public string CreatorId { get; set; }
        public string CreatorName { get; set; }
        public string CreatorEmail { get; set; }
        public string CreationDateString { get; set; }
        public List<string> userGrants { get; set; }

    }
}
