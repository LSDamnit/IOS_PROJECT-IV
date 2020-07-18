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
            public const string Edit = "Grant.Institutions.Edit";
            public const string Create = "Grant.Institutions.Create";
            public const string Delete = "Grant.Institutions.Delete";
        }
        public static class Departments
        {
            public const string Edit = "Grant.Departments.Edit";
            public const string Create = "Grant.Departments.Create";
            public const string Delete = "Grant.Departments.Delete";
        }
        public static class Specialities
        {
            public const string Create = "Grant.Specialities.Create";
            public const string Delete = "Grant.Specialities.Delete";
            public const string Edit = "Grant.Specialities.Edit";
        }
        public static class Disciplines
        {
            public const string EnrollStudent = "Grant.Disciplines.EnrollStudent";
            public const string Create = "Grant.Disciplines.Create";
            public const string Delete = "Grant.Disciplines.Delete";
            public const string Edit = "Grant.Disciplines.Edit"; 
        }
        public static class DisciplinesDetails
        {
            public const string Files = "Grant.DisciplinesDetails.Files";
            public const string FilePath = "Grant.DisciplinesDetails.FilePath";
        }
        public static class Schedule
        {
            public const string Create = "Grant.Schedule.Create";
            public const string Delete = "Grant.Schedule.Delete";
            public const string Edit = "Grant.Schedule.Edit";
        }
        public static class UsersAdmin
        {
            public const string View = "Grant.UsersAdmin.View";
            public const string CreateUsers = "Grant.UsersAdmin.CreateUsers";
            public const string Edit = "Grant.UsersAdmin.Edit";
            public const string Delete = "Grant.UsersAdmin.Delete";
            public const string ResetPassword = "Grant.UsersAdmin.ResetPassword";
            public const string Roles = "Grant.UsersAdmin.Roles";
        }
        public static class Message
        {
            public const string Admin = "Grant.Message.Admin";
            public const string Departments = "Grant.Message.Departments";
            public const string Specialities = "Grant.Message.Specialities";
            public const string Disciplines = "Grant.Message.Disciplines";
            public const string DisciplinesDetails = "Grant.Message.DisciplinesDetails";
        }
        public static class Complains
		{
            public const string View = "Grants.Complains.View";
            public const string Details = "Grants.Complains.Details";

        }
        public static class Forum
        {
            public const string EditNode = "Grants.Complains.EditNode";
            public const string CreateNode = "Grants.Complains.CreateNode";
            public const string EditEndpoint = "Grants.Complains.EditEndpoint";
            public const string DeleteComment = "Grants.Complains.DeleteComment";

        }
    }
}
