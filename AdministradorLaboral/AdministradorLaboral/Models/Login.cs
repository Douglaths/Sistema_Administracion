using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AdministradorLaboral.Models
{
    public class Login
    {
        [Required(ErrorMessage = "The username is required.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "The password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}