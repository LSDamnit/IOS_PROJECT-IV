using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IOS_PROJECT3.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IOS_PROJECT3.ViewModels
{
	public class EditRoleViewModel
	{
		public string roleId { get; set; }
		[Required(ErrorMessage = "Название не может быть пустым")]
		public string Name { get; set; }

		public List<EGrant> allGrants { get; set; }
		public List<string> roleGrantsId { get; set; }
	}
}
