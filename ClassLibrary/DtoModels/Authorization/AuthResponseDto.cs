using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary.DtoModels.Authorization
{
    public class AuthResponseDto
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public int AdminId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
