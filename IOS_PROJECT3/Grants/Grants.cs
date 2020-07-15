using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.Grants
{
	public static class Grants
	{
        public static class Roles
        {
            public const string View = "Grant.Roles.View";
            public const string Create = "Grant.Roles.Create";
            public const string Edit = "Grant.Roles.Edit";
            public const string EditRole = "Grant.Roles.EditRole";
            public const string Delete = "Grant.Roles.Delete";
        }

        public static class Institutions
        {
            public const string View = "Grant.Institutions.View";
        }
        public static class UsersAdmin
        {
            public const string View = "Grant.UsersAdmin.View";
        }
        public static class Specialities
        {
            public const string View = "Grant.Specialities.View";
        }
    }
}
