using IOS_PROJECT3.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class EditDisciplineViewModel
    {
        public string DisciplineId { get; set; }
        public string SpecialityId { get; set; }

        [Required(ErrorMessage = "Не указано название")]
        public string Name { get; set; }
        public int LectionH { get; set; }
        public int PracticeH { get; set; }
        [Required(ErrorMessage = "Не указан тип зачета")]
        public string ExamType { get; set; }
        public IList<EUser> AvailableTeachers { get; set; }
        public string Info { get; set; }
        public string TeacherId { get; set; }
    }
}
