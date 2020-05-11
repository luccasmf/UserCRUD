﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace UserCRUD.ViewModels
{
    public class RegisterUserViewModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]

        public string Email { get; set; }
        [Required]

        public string Password { get; set; }
    }
}
