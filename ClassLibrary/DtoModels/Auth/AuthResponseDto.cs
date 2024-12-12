using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Auth
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
