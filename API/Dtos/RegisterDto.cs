using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string  KnownAs { get; set; }

       [Required]
        public string Gender{ get; set; }
        [Required]
        public DateOnly? DateOfBirth { get; set; }
        [Required]
        public string City { get; set; }
         public String Country { get; set; }
        [Required]
        [StringLength(8, MinimumLength =4)]
        public string Password { get; set; }
    }
}