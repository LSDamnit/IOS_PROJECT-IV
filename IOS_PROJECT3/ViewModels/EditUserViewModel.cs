using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IOS_PROJECT3.ViewModels
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        [Required(ErrorMessage ="Не указан Email")]
        
        public string Email { get; set; }
     /*   [Required]
        [Display(Name = "Пароль")]
        [DataType(DataType.Password)]*/
        public string Password { get; set; }

        [Required(ErrorMessage = "Не указано ФИО")]
        public string FIO { get; set; }
    }
}
