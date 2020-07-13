using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace IOS_PROJECT3.Grants
{
	internal class GrantRequirement : IAuthorizationRequirement
	{
		public string Grant { get; private set; }

		public GrantRequirement(string Grant)
		{
			this.Grant = Grant;
		}
	}
}
